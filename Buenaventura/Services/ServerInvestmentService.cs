using Buenaventura.Api;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
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
            Investments = new List<InvestmentModel>(),
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

    public async Task DeleteInvestment(Guid investmentId)
    {
        var context = await contextFactory.CreateDbContextAsync();
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
        var context = await contextFactory.CreateDbContextAsync();
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

        await CreateInvestmentTransaction(investmentModel, investment, context);
        await context.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private async Task CreateInvestmentTransaction(AddInvestmentModel investmentDto,
        Investment investment, BuenaventuraDbContext context)
    {
        if (investmentDto.AccountId == null || investmentDto.Date == null)
        {
            throw new Exception("Account ID and Date are required");
        }

        var buySell = investmentDto.Shares > 0
            ? $"Buy {investmentDto.Shares} share"
            : $"Sell {investmentDto.Shares} share";
        if (investmentDto.Shares != 1) buySell += "s";
        var description = $"Investment: {buySell} of {investmentDto.Symbol} at {investmentDto.Price:N2}";
        var investmentAccount =
            await context.Accounts.FirstAsync(a => a.AccountType == "Investment").ConfigureAwait(false);
        var enteredDate = DateTime.Now;
        var exchangeRate = await context.Currencies.GetCadExchangeRate();
        var investmentAccountTransaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = investmentAccount.AccountId,
            Amount = Math.Round(investmentDto.Shares * investmentDto.Price, 2),
            TransactionDate = investmentDto.Date.Value,
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
            AccountId = investmentDto.AccountId.Value,
            Amount = 0 - Math.Round(investmentDto.Shares * investmentDto.Price, 2),
            TransactionDate = investmentDto.Date.Value,
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
            Price = investmentDto.Price,
            Date = investmentDto.Date.Value,
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
    }
}