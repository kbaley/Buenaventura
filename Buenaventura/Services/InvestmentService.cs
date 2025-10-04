using Buenaventura.Api;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IInvestmentService : IAppService
{
    Task<InvestmentListModel> GetInvestments();
    Task<InvestmentListModel> UpdateCurrentPrices();
    
    // Makes required entry in the investments account to match the total of the portfolio
    Task MakeCorrectingEntry();
    Task DeleteInvestment(Guid investmentId);
    Task AddInvestment(AddInvestmentModel investmentModel);
    Task BuySell(BuySellModel buySellModel);
    Task RecordDividend(Guid investmentId, RecordDividendModel model);
}

public class InvestmentService(
    BuenaventuraDbContext context,
    IInvestmentPriceParser priceParser,
    IInvestmentTransactionGenerator transactionGenerator,
    ITransactionRepository transactionRepo,
    ICurrencyService currencyService
) : IInvestmentService
{
    public async Task<InvestmentListModel> GetInvestments()
    {
        var investments = await context.Investments
            .Include(i => i.Transactions)
            .Include(i => i.Dividends)
            .Select(i => new InvestmentModel
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
            i.AveragePrice = (i.Shares == 0 || i.TotalSharesBought == 0) ? 0 : i.AveragePrice / i.TotalSharesBought;
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
        var mustUpdatePrices = context.Investments
            .Any(i => !i.DontRetrievePrices && i.LastPriceRetrievalDate < DateTime.Today);
        if (mustUpdatePrices)
        {
            await priceParser.UpdatePricesFor(context);
        }

        if (mustUpdatePrices)
            return await GetInvestments();
        return new InvestmentListModel
        {
            Investments = new List<InvestmentModel>(),
            PortfolioIrr = await context.Investments.GetAnnualizedIrr()
        };
    }

    public async Task MakeCorrectingEntry()
    {
        var investments = context.Investments
            .Include(i => i.Transactions);

        var investmentsTotal = investments
            .Where(i => i.Currency == "USD").ToList()
            .Sum(i => i.GetCurrentValue());
        if (investments
            .Any(i => i.Currency == "CAD" && i.Transactions.Sum(t => t.Shares) > 0))
        {
            var exchangeRate = await currencyService.GetExchangeRateFor("CAD");
            investmentsTotal += investments
                .Where(i => i.Currency == "CAD").ToList()
                .Sum(i => i.GetCurrentValue() / exchangeRate);
        }
        var investmentAccount = context.Accounts.FirstOrDefault(a => a.AccountType == "Investment");
        if (investmentAccount == null)
            return;

        var bookBalance = context.Transactions
            .Where(t => t.AccountId == investmentAccount.AccountId).ToList()
            .Sum(i => i.Amount);

        var difference = Math.Round(investmentsTotal - bookBalance, 2);
        if (Math.Abs(difference) >= 1)
        {
            var category = await context.GetOrCreateCategory("Gain/loss on investments");
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

    public async Task DeleteInvestment(Guid investmentId)
    {
        await using var tx = await context.Database.BeginTransactionAsync();
        var investment = await context.Investments
            .Include(i => i.Transactions)
            .ThenInclude(t => t.Transaction)
            .ThenInclude(t => t.LeftTransfer)
            .ThenInclude(t => t!.RightTransaction)
            .ThenInclude(t => t!.LeftTransfer)
            .SingleAsync(i => i.InvestmentId == investmentId);
        foreach (var transaction in investment.Transactions)
        {
            context.Transactions.Remove(transaction.Transaction.LeftTransfer!.RightTransaction!);
            context.Transactions.Remove(transaction.Transaction);
            context.Transfers.Remove(transaction.Transaction.LeftTransfer);
            context.Transfers.Remove(transaction.Transaction.LeftTransfer.RightTransaction!.LeftTransfer!);
        }

        var dividendTransactions = context.Transactions
            .Where(t => t.DividendInvestmentId == investmentId);
        foreach (var dividendTransaction in dividendTransactions)
        {
            context.Transactions.Remove(dividendTransaction);
        }

        await context.SaveChangesAsync();

        context.Investments.Remove(investment);
        await context.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task AddInvestment(AddInvestmentModel investmentModel)
    {
        await using var tx = await context.Database.BeginTransactionAsync();
        // Check if we've bought this investment before
        var investment = await context.Investments
            .SingleOrDefaultAsync(i => i.Symbol == investmentModel.Symbol);
        if (investment == null)
        {
            investment = new Investment
            {
                InvestmentId = Guid.NewGuid(),
                Name = investmentModel.Name,
                Symbol = investmentModel.Symbol,
                Currency = investmentModel.Currency,
                LastPrice = investmentModel.Price,
                CategoryId = investmentModel.CategoryId,
                PaysDividends = investmentModel.PaysDividends,
                DontRetrievePrices = investmentModel.DontRetrievePrices,
                LastPriceRetrievalDate = DateTime.UtcNow
            };
            await context.Investments.AddAsync(investment);
        }
        else
        {
            investment.LastPrice = investmentModel.Price;
            investment.LastPriceRetrievalDate = DateTime.UtcNow;
            investment.CategoryId = investmentModel.CategoryId;
            investment.PaysDividends = investmentModel.PaysDividends;
            investment.DontRetrievePrices = investmentModel.DontRetrievePrices;
            investment.Name = investmentModel.Name;
            investment.Currency = investmentModel.Currency;
            context.Investments.Update(investment);
        }

        await transactionGenerator.CreateInvestmentTransaction(investmentModel, investment, context);
        await context.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task BuySell(BuySellModel model)
    {
        var investment = await context.Investments.SingleAsync(i => i.InvestmentId == model.InvestmentId);
        await transactionGenerator.CreateInvestmentTransaction(model, investment!, context);
        await context.SaveChangesAsync();
    }

    public async Task RecordDividend(Guid investmentId, RecordDividendModel model)
    {
        await using var tx = await context.Database.BeginTransactionAsync();
        var investmentIncomeCategory = await context.Categories
            .SingleAsync(c => c.Name == "Investment Income");
        var incomeTaxCategory = await context.Categories
            .SingleAsync(c => c.Name == "Income Tax");
        var now = DateTime.Now;
        var exchangeRate = await context.Currencies.GetCadExchangeRate();
        var accountCurrency = (await context.Accounts.FindAsync(model.AccountId))!.Currency;
        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = model.AccountId,
            Amount = Math.Round(model.Amount, 2),
            TransactionDate = model.Date,
            EnteredDate = now,
            Description = model.Description + " (DIVIDEND)",
            TransactionType = TransactionType.DIVIDEND,
            DividendInvestmentId = investmentId,
            CategoryId = investmentIncomeCategory.CategoryId,
        };
        transaction.SetAmountInBaseCurrency(accountCurrency, exchangeRate);
        context.Transactions.Add(transaction);
        if (model.IncomeTax != 0)
        {
            var taxTransaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                AccountId = model.AccountId,
                Amount = -Math.Round(model.IncomeTax, 2),
                TransactionDate = model.Date,
                EnteredDate = now,
                Description = model.Description + " (INCOME TAX)",
                TransactionType = TransactionType.DIVIDEND,
                DividendInvestmentId = investmentId,
                CategoryId = incomeTaxCategory.CategoryId,
            };
            taxTransaction.SetAmountInBaseCurrency(accountCurrency, exchangeRate);
            context.Transactions.Add(taxTransaction);
        }

        await context.SaveChangesAsync();
        await tx.CommitAsync();
    }
}