using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IInvestmentTransactionGenerator : IServerAppService
{

     Task CreateInvestmentTransaction(AddInvestmentModel model, Investment investment,
          BuenaventuraDbContext context);
     Task CreateInvestmentTransaction(BuySellModel model, Investment investment,
          BuenaventuraDbContext context);
}

public class InvestmentTransactionGenerator : IInvestmentTransactionGenerator
{
     public async Task CreateInvestmentTransaction(AddInvestmentModel model, Investment investment, BuenaventuraDbContext context)
     {
        if (model.AccountId == null || model.Date == null)
        {
            throw new Exception("Account ID and Date are required");
        }

        var buySellModel = new BuySellModel
        {
            InvestmentId = investment.InvestmentId,
            Shares = model.Shares,
            Price = model.Price,
            Date = model.Date.Value,
            AccountId = model.AccountId.Value
        };
        await CreateInvestmentTransaction(buySellModel, investment, context);

     }

     public async Task CreateInvestmentTransaction(BuySellModel model, Investment investment, BuenaventuraDbContext context)
     {
        var buySell = model.Shares > 0
            ? $"Buy {model.Shares} share"
            : $"Sell {model.Shares} share";
        if (model.Shares != 1) buySell += "s";
        var description = $"Investment: {buySell} of {investment.Symbol} at {model.Price:N2}";
        var investmentAccount =
            await context.Accounts.FirstAsync(a => a.AccountType == "Investment").ConfigureAwait(false);
        var enteredDate = DateTime.Now;
        var exchangeRate = await context.Currencies.GetCadExchangeRate();
        var investmentAccountTransaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = investmentAccount.AccountId,
            Amount = Math.Round(model.Shares * model.Price, 2),
            TransactionDate = model.Date,
            EnteredDate = enteredDate,
            TransactionType = TransactionType.INVESTMENT,
            Description = description
        };
        investmentAccountTransaction.SetAmountInBaseCurrency(investmentAccount.Currency, exchangeRate);
        var otherAccount = await context.Accounts.FindAsync(model.AccountId);
        var currency = otherAccount!.Currency;
        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = model.AccountId,
            Amount = 0 - Math.Round(model.Shares * model.Price, 2),
            TransactionDate = model.Date,
            EnteredDate = enteredDate,
            TransactionType = TransactionType.INVESTMENT,
            Description = description
        };
        transaction.SetAmountInBaseCurrency(currency, exchangeRate);
        var investmentTransaction = new InvestmentTransaction
        {
            InvestmentTransactionId = Guid.NewGuid(),
            InvestmentId = investment.InvestmentId,
            Shares = model.Shares,
            Price = model.Price,
            Date = model.Date,
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