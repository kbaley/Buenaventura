using Buenaventura.Api;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerInvestmentService(
    IDbContextFactory<BuenaventuraDbContext> contextFactory,
    IInvestmentPriceParser priceParser,
    ITransactionRepository transactionRepo
) : IInvestmentService
{
    public async Task<InvestmentListModel> GetInvestments()
    {
        var context = await contextFactory.CreateDbContextAsync();
        var investments = await context.Investments
            .Include(i => i.Transactions)
            .Include(i => i.Dividends)
            .Select(i => new InvestmentForListDto
            {
                InvestmentId = i.InvestmentId,
                Name = i.Name,
                Symbol = i.Symbol,
                Shares = i.Transactions.Sum(t => t.Shares),
                TotalSharesBought = i.Transactions.Where(t => t.Shares > 0).Sum(t => t.Shares),
                LastPrice = i.LastPrice,
                // Don't divide by number of shares yet in case it's zero; we'll do this later
                AveragePrice = i.Transactions.Where(t => t.Shares > 0).Sum(t => t.Shares * t.Price),
                Currency = i.Currency,
                DontRetrievePrices = i.DontRetrievePrices,
                AnnualizedIrr = i.GetAnnualizedIrr(),
                CategoryId = i.CategoryId,
                PaysDividends = i.PaysDividends
            })
            .Where(i => i.Shares != 0)
            .OrderBy(i => i.Name)
            .ToListAsync();
        investments.ForEach(i =>
        {
            i.CurrentValue = i.Shares * i.LastPrice;
            i.AveragePrice = i.Shares == 0 ? 0 : i.AveragePrice / i.TotalSharesBought;
            i.BookValue = i.Shares * i.AveragePrice;
        });
        var totalIrr = await context.Investments.GetAnnualizedIrr();

        return new InvestmentListModel
        {
            Investments = investments,
            PortfolioIrr = totalIrr
        };
    }

    public async Task<InvestmentListModel> UpdateCurrentPrices()
    {
        var context = await contextFactory.CreateDbContextAsync();
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

    public async Task MakeCorrectingEntry()
    {
        var context = await contextFactory.CreateDbContextAsync();
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
            return;

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
                Category = new CategoryModel
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
        }
    }
}