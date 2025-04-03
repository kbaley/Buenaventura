using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Dtos;
using Buenaventura.Shared;

namespace Buenaventura.Services;

public class ServerAccountService(
    CoronadoDbContext context,
    ITransactionRepository transactionRepo
    ) : IAccountService
{
    public Task<IEnumerable<AccountWithBalance>> GetAccounts()
    {
        var exchangeRate = context.Currencies.GetCadExchangeRate();
        var accounts = context.Accounts
            .Select(a => new AccountWithBalance
            {
                AccountId = a.AccountId,
                Name = a.Name,
                Currency = a.Currency,
                Vendor = a.Currency,
                AccountType = a.AccountType,
                MortgagePayment = a.MortgagePayment,
                MortgageType = a.MortgageType,
                DisplayOrder = a.DisplayOrder,
                IsHidden = a.IsHidden,
                CurrentBalance = a.Transactions.Sum(t => t.Amount),
                CurrentBalanceInUsd = a.Currency == "CAD" 
                    ? Math.Round(a.Transactions.Sum(t => t.Amount) / exchangeRate, 2)
                    : a.Transactions.Sum(t => t.Amount),
            });
        return Task.FromResult<IEnumerable<AccountWithBalance>>(accounts);
    }

    public Task<TransactionListModel> GetTransactions(Guid accountId, bool loadAll = false)
    {
        if (loadAll) {
            var transactions = transactionRepo.GetByAccount(accountId);
            return Task.FromResult(new TransactionListModel {
                Transactions = transactions,
                StartingBalance = 0,
                RemainingTransactionCount = 0
            });
        }
        return Task.FromResult(transactionRepo.GetByAccount(accountId, 0));
    }
}