using AutoMapper;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Dtos;
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
        CoronadoDbContext context,
        ITransactionRepository transactionRepo,
        IInvestmentPriceParser priceParser,
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
                .SingleOrDefaultAsync(i => i.InvestmentId == investmentId).ConfigureAwait(false);
            if (investment == null)
            {
                return NotFound();
            }
            await context.Entry(investment).Collection(i => i.Transactions).LoadAsync().ConfigureAwait(false);
            var dividends = GetDividendDtosFrom(investment);
            var mappedInvestment = mapper.Map<InvestmentDetailDto>(investment);
            mappedInvestment.Transactions = mappedInvestment.Transactions.OrderBy(t => t.Date);
            mappedInvestment.TotalPaid = Math.Round(investment.Transactions.Sum(t => t.Shares * t.Price), 2);
            mappedInvestment.CurrentValue = Math.Round(mappedInvestment.LastPrice * mappedInvestment.Shares);
            mappedInvestment.BookValue = Math.Round(mappedInvestment.AveragePrice * mappedInvestment.Shares);
            mappedInvestment.Dividends = dividends;

            return mappedInvestment;
        }

        private IEnumerable<InvestmentDividendDto> GetDividendDtosFrom(Investment investment) {

            var dividendTransactions = investment.Dividends
                .OrderBy(d => d.TransactionDate)
                .ThenBy(d => d.EnteredDate)
                .ThenBy(d => d.Amount)
                .ToList();
            var dividends = new List<InvestmentDividendDto>();
            var i = 0;

            // Some dividends have income tax; the sort order means we'll get all dividends
            // order by transaction date, then it will be pairs of transactions with the first one
            // being the tax (the amount is < 0) and the second being the actual dividend
            while (i < dividendTransactions.Count) {
                var dividend = new InvestmentDividendDto
                {
                    Date = dividendTransactions[i].TransactionDate,
                };
                if (dividendTransactions[i].Amount < 0) {
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
            return await investmentService.GetInvestments();
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
            await context.SaveChangesAsync().ConfigureAwait(false);
            return await GetInvestments();
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<InvestmentListModel> UpdateCurrentPrices()
        {
            var mustUpdatePrices = context.Investments
                .Any(i => !i.DontRetrievePrices && i.LastPriceRetrievalDate < DateTime.Today);
            if (mustUpdatePrices)
            {
                await priceParser.UpdatePricesFor(context).ConfigureAwait(false);
            }
            if (mustUpdatePrices)
                return await GetInvestments();
            return new InvestmentListModel
            {
                Investments = new List<InvestmentForListDto>(),
                PortfolioIrr = await context.Investments.GetAnnualizedIrr()
            };
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> RecordDividend(InvestmentDividendDto investmentDto)
        {

            var investment = await context.Investments.FindAsync(investmentDto.InvestmentId);
            var investmentIncomeCategory = await context.Categories
                .SingleAsync(c => c.Name == "Investment Income");
            var incomeTaxCategory = await context.Categories
                .SingleAsync(c => c.Name == "Income Tax");
            var now = DateTime.Now;
            var exchangeRate = await context.Currencies.GetCadExchangeRate();
            var accountCurrency = (await context.Accounts.FindAsync(investmentDto.AccountId))!.Currency;
            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                AccountId = investmentDto.AccountId,
                Amount = Math.Round(investmentDto.Amount, 2),
                TransactionDate = investmentDto.Date,
                EnteredDate = now,
                Description = investmentDto.Description + " (DIVIDEND)",
                TransactionType = TransactionType.DIVIDEND,
                DividendInvestmentId = investmentDto.InvestmentId,
                CategoryId = investmentIncomeCategory.CategoryId,
            };
            transaction.SetAmountInBaseCurrency(accountCurrency, exchangeRate);
            context.Transactions.Add(transaction);
            if (investmentDto.IncomeTax != 0)
            {
                var taxTransaction = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    AccountId = investmentDto.AccountId,
                    Amount = -Math.Round(investmentDto.IncomeTax, 2),
                    TransactionDate = investmentDto.Date,
                    EnteredDate = now,
                    Description = investmentDto.Description + " (INCOME TAX)",
                    TransactionType = TransactionType.DIVIDEND,
                    DividendInvestmentId = investmentDto.InvestmentId,
                    CategoryId = incomeTaxCategory.CategoryId,
                };
                taxTransaction.SetAmountInBaseCurrency(accountCurrency, exchangeRate);
                context.Transactions.Add(taxTransaction);
            }
            await context.SaveChangesAsync();

            return Ok(investment);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> BuySell(InvestmentForListDto investmentDto)
        {

            var investment = await context.Investments.FindAsync(investmentDto.InvestmentId).ConfigureAwait(false);
            await CreateInvestmentTransaction(investmentDto, investment!).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);

            return Ok(investment);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> MakeCorrectingEntries()
        {
            var investments = context.Investments
                .Include(i => i.Transactions);
            var currencyController = new CurrenciesController(context);
            var currency = currencyController.GetExchangeRateFor("CAD").GetAwaiter().GetResult();
            var investmentsTotal = investments
                .Where(i => i.Currency == "CAD").ToList()
                .Sum(i => i.GetCurrentValue() / currency);
            investmentsTotal += investments
                .Where(i => i.Currency == "USD").ToList()
                .Sum(i => i.GetCurrentValue());
            var investmentAccount = context.Accounts.FirstOrDefault(a => a.AccountType == "Investment");
            if (investmentAccount == null)
                return Ok();

            var bookBalance = context.Transactions
                .Where(t => t.AccountId == investmentAccount.AccountId).ToList()
                .Sum(i => i.Amount);

            var difference = Math.Round(investmentsTotal - bookBalance, 2);
            if (Math.Abs(difference) >= 1)
            {
                var category = await context.GetOrCreateCategory("Gain/loss on investments").ConfigureAwait(false);
                var transaction = new TransactionForDisplay
                {
                    TransactionId = Guid.NewGuid(),
                    AccountId = investmentAccount.AccountId,
                    Amount = difference,
                    Category = new CategoryDto
                    {
                        CategoryId = category.CategoryId,
                        Name = category.Name,
                    },
                    TransactionDate = DateTime.Now,
                    EnteredDate = DateTime.Now,
                    Description = ""
                };
                transaction.SetDebitAndCredit();
                await transactionRepo.Insert(transaction);
                var accountBalances = (await context.GetAccountBalances()).ToList();
                var transactions = new[] { transaction };

                return CreatedAtAction("MakeCorrectingEntries", new { id = transaction.TransactionId }, new { transactions, accountBalances });
            }
            else
            {
                return Ok();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvestment([FromRoute] Guid id, [FromBody] InvestmentForUpdateDto investment)
        {
            if (id != investment.InvestmentId)
            {
                return BadRequest();
            }
            // Don't update the price
            var investmentFromDb = await context.Investments.FindAsync(investment.InvestmentId).ConfigureAwait(false);
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
            await context.Entry(investmentMapped).ReloadAsync().ConfigureAwait(false);
            await context.Entry(investmentMapped).Collection(i => i.Transactions).LoadAsync().ConfigureAwait(false);
            var returnInvestment = mapper.Map<InvestmentForListDto>(investmentMapped);

            return Ok(returnInvestment);
        }

        [HttpPost]
        public async Task<IActionResult> PostInvestment([FromBody] InvestmentForListDto investmentDto)
        {
            var investment = await context.Investments.SingleOrDefaultAsync(i => i.Symbol == investmentDto.Symbol).ConfigureAwait(false);
            if (investment == null)
            {
                investmentDto.InvestmentId = Guid.NewGuid();
                var mappedInvestment = mapper.Map<Investment>(investmentDto);
                investment = context.Investments.Add(mappedInvestment).Entity;
            }
            var investmentTransaction = await CreateInvestmentTransaction(investmentDto, investment).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);

            var accountBalances = (await context.GetAccountBalances()).ToList();
            return CreatedAtAction("PostInvestment", new { id = investment.InvestmentId },
                new
                {
                    investment,
                    investmentTransaction,
                    accountBalances
                }
            );
        }

        private async Task<InvestmentTransaction> CreateInvestmentTransaction(InvestmentForListDto investmentDto, Investment investment)
        {
            var buySell = investmentDto.Shares > 0 ? $"Buy {investmentDto.Shares} share" : $"Sell {investmentDto.Shares} share";
            if (investmentDto.Shares != 1) buySell += "s";
            var description = $"Investment: {buySell} of {investmentDto.Symbol} at {investmentDto.LastPrice}";
            var investmentAccount = await context.Accounts.FirstAsync(a => a.AccountType == "Investment").ConfigureAwait(false);
            var enteredDate = DateTime.Now;
            var exchangeRate = await context.Currencies.GetCadExchangeRate();
            var investmentAccountTransaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                AccountId = investmentAccount.AccountId,
                Amount = Math.Round(investmentDto.Shares * investmentDto.LastPrice, 2),
                TransactionDate = investmentDto.Date,
                EnteredDate = enteredDate,
                TransactionType = TransactionType.INVESTMENT,
                Description = description
            };
            investmentAccountTransaction.SetAmountInBaseCurrency(investmentAccount.Currency, exchangeRate);
            var otherAccount = await context.Accounts.FindAsync(investmentDto.AccountId);
            var currency = otherAccount!.Currency;
            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                AccountId = investmentDto.AccountId,
                Amount = 0 - Math.Round(investmentDto.Shares * investmentDto.LastPrice, 2),
                TransactionDate = investmentDto.Date,
                EnteredDate = enteredDate,
                TransactionType = TransactionType.INVESTMENT,
                Description = description
            };
            transaction.SetAmountInBaseCurrency(currency, exchangeRate);
            var investmentTransaction = new InvestmentTransaction
            {
                InvestmentTransactionId = Guid.NewGuid(),
                InvestmentId = investment.InvestmentId,
                Shares = investmentDto.Shares,
                Price = investmentDto.LastPrice,
                Date = investmentDto.Date,
                TransactionId = transaction.TransactionId
            };
            context.Transactions.Add(investmentAccountTransaction);
            context.Transactions.Add(transaction);
            context.InvestmentTransactions.Add(investmentTransaction);
            context.Transfers.Add(new Transfer
            {
                TransferId = Guid.NewGuid(),
                LeftTransactionId = transaction.TransactionId,
                RightTransactionId = investmentAccountTransaction.TransactionId
            });
            context.Transfers.Add(new Transfer
            {
                TransferId = Guid.NewGuid(),
                RightTransactionId = transaction.TransactionId,
                LeftTransactionId = investmentAccountTransaction.TransactionId
            });
            return investmentTransaction;
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] Guid id)
        {
            var investment = context.Investments
                .Include(i => i.Transactions)
                .ThenInclude(t => t.Transaction)
                .ThenInclude(t => t.LeftTransfer)
                .ThenInclude(t => t.RightTransaction)
                .ThenInclude(t => t!.LeftTransfer)
                .Single(i => i.InvestmentId == id);
            foreach (var transaction in investment.Transactions)
            {
                context.Transactions.Remove(transaction.Transaction.LeftTransfer.RightTransaction!);
                context.Transactions.Remove(transaction.Transaction);
                context.Transfers.Remove(transaction.Transaction.LeftTransfer);
                context.Transfers.Remove(transaction.Transaction.LeftTransfer.RightTransaction!.LeftTransfer);
            }

            context.Investments.Remove(investment);
            context.SaveChanges();
            return Ok(investment);
        }
    }
}
