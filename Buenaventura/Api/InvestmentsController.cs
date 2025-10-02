using AutoMapper;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Dtos;
using Buenaventura.Services;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InvestmentsController(
        BuenaventuraDbContext context,
        IInvestmentService investmentService,
        IMapper mapper)
        : ControllerBase
    {
        [HttpGet("{investmentId}")]
        public async Task<ActionResult<InvestmentDetailDto>> Get(Guid investmentId)
        {
            var investment = await context.Investments
                .Include(i => i.Dividends)
                .Include(i => i.Transactions)
                .ThenInclude(t => t.Transaction.Account)
                .SingleOrDefaultAsync(i => i.InvestmentId == investmentId);
            if (investment == null)
            {
                return NotFound();
            }

            await context.Entry(investment).Collection(i => i.Transactions).LoadAsync();
            var dividends = GetDividendDtosFrom(investment);
            var mappedInvestment = mapper.Map<InvestmentDetailDto>(investment);
            mappedInvestment.Transactions = mappedInvestment.Transactions.OrderBy(t => t.Date);
            mappedInvestment.TotalPaid = Math.Round(investment.Transactions.Sum(t => t.Shares * t.Price), 2);
            mappedInvestment.CurrentValue = Math.Round(mappedInvestment.LastPrice * mappedInvestment.Shares);
            mappedInvestment.BookValue = Math.Round(mappedInvestment.AveragePrice * mappedInvestment.Shares);
            mappedInvestment.Dividends = dividends;

            return mappedInvestment;
        }

        private IEnumerable<RecordDividendModel> GetDividendDtosFrom(Investment investment)
        {
            var dividendTransactions = investment.Dividends
                .OrderBy(d => d.TransactionDate)
                .ThenBy(d => d.EnteredDate)
                .ThenBy(d => d.Amount)
                .ToList();
            var dividends = new List<RecordDividendModel>();
            var i = 0;

            // Some dividends have income tax; the sort order means we'll get all dividends
            // order by transaction date, then it will be pairs of transactions with the first one
            // being the tax (the amount is < 0) and the second being the actual dividend
            while (i < dividendTransactions.Count)
            {
                var dividend = new RecordDividendModel
                {
                    Date = dividendTransactions[i].TransactionDate,
                };
                if (dividendTransactions[i].Amount < 0)
                {
                    dividend.IncomeTax = -dividendTransactions[i++].Amount;
                }

                dividend.Amount = dividendTransactions[i++].Amount;
                dividend.Total = dividend.Amount - dividend.IncomeTax;

                dividends.Add(dividend);
            }

            return dividends;
        }

        [HttpGet]
        public async Task<InvestmentListModel> GetInvestments()
        {
            var investments = await investmentService.GetInvestments();
            return investments;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<InvestmentListModel> SaveTodaysPrices(IEnumerable<TodaysPriceDto> investments)
        {
            var investmentsFromDb = context.Investments.ToList();
            foreach (var item in investments)
            {
                var investment = investmentsFromDb.SingleOrDefault(i => i.InvestmentId == item.InvestmentId);
                if (investment != null)
                {
                    investment.LastPriceRetrievalDate = DateTime.Today;
                    investment.LastPrice = item.LastPrice;
                }
            }

            await context.SaveChangesAsync();
            return await GetInvestments();
        }

        [HttpPost]
        [Route("updatecurrentprices")]
        public async Task<InvestmentListModel> UpdateCurrentPrices()
        {
            return await investmentService.UpdateCurrentPrices();
        }

        [HttpPost]
        [Route("{investmentId}/dividends")]
        public async Task RecordDividend(Guid investmentId, [FromBody] RecordDividendModel model)
        {
            await investmentService.RecordDividend(investmentId, model);
        }

        [HttpPost]
        [Route("buysell")]
        public async Task BuySell(BuySellModel model)
        {
            await investmentService.BuySell(model);
        }

        [HttpPost]
        [Route("makecorrectingentry")]
        public async Task<IActionResult> MakeCorrectingEntry()
        {
            await investmentService.MakeCorrectingEntry();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvestment([FromRoute] Guid id,
            [FromBody] InvestmentForUpdateDto investment)
        {
            if (id != investment.InvestmentId)
            {
                return BadRequest();
            }

            // Don't update the price
            var investmentFromDb = await context.Investments.FindAsync(investment.InvestmentId);
            context.Entry(investmentFromDb).State = EntityState.Detached;
            var lastPrice = investmentFromDb!.LastPrice;
            var lastPriceRetrievalDate = investmentFromDb.LastPriceRetrievalDate;

            var investmentMapped = mapper.Map<Investment>(investment);
            investmentMapped.LastPrice = lastPrice;
            investmentMapped.LastPriceRetrievalDate = lastPriceRetrievalDate;
            context.Entry(investmentMapped).State = EntityState.Modified;
            if (investmentMapped.CategoryId == Guid.Empty)
            {
                investmentMapped.CategoryId = null;
            }

            await context.SaveChangesAsync();
            await context.Entry(investmentMapped).ReloadAsync();
            await context.Entry(investmentMapped).Collection(i => i.Transactions).LoadAsync();
            var returnInvestment = mapper.Map<InvestmentModel>(investmentMapped);

            return Ok(returnInvestment);
        }

        [HttpPost]
        public async Task PostInvestment([FromBody] AddInvestmentModel investmentDto)
        {
            await investmentService.AddInvestment(investmentDto);
        }

        [HttpDelete("{id}")]
        public async Task Delete([FromRoute] Guid id)
        {
            await investmentService.DeleteInvestment(id);
        }
    }
}