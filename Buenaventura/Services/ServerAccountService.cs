using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerAccountService(
    BuenaventuraDbContext context,
    ITransactionRepository transactionRepo
) : IAccountService
{
    public async Task<IEnumerable<AccountWithBalance>> GetAccounts()
    {
        var exchangeRate = await context.Currencies.GetCadExchangeRate();
        var accounts = await context.Accounts
            .Where(a => !a.IsHidden)
            .OrderBy(a => a.DisplayOrder)
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
            }).ToListAsync();
        return accounts;
    }

    public async Task<TransactionListModel> GetTransactions(Guid accountId, string search = "", int page = 0,
        int pageSize = 50)
    {
        return await transactionRepo.GetByAccount(accountId, search, page, pageSize);
    }

    public async Task<AccountWithBalance> GetAccount(Guid id)
    {
        var exchangeRate = await context.Currencies.GetCadExchangeRate();
        var account = await context.Accounts.Include(account => account.Transactions)
            .FirstOrDefaultAsync(a => a.AccountId == id);
        if (account == null)
        {
            return new AccountWithBalance();
        }

        var accountWithBalance = new AccountWithBalance
        {
            AccountId = account.AccountId,
            Name = account.Name,
            Currency = account.Currency,
            Vendor = account.Currency,
            AccountType = account.AccountType,
            MortgagePayment = account.MortgagePayment,
            MortgageType = account.MortgageType,
            DisplayOrder = account.DisplayOrder,
            IsHidden = account.IsHidden,
            CurrentBalance = account.Transactions.Sum(t => t.Amount),
            CurrentBalanceInUsd = account.Currency == "CAD"
                ? Math.Round(account.Transactions.Sum(t => t.Amount) / exchangeRate, 2)
                : account.Transactions.Sum(t => t.Amount),
        };
        return accountWithBalance;
    }

    public async Task UpdateTransaction(TransactionForDisplay transaction)
    {
        transaction.SetAmount();
        await transactionRepo.Update(transaction);
    }

    public async Task AddTransaction(Guid accountId, TransactionForDisplay transaction)
    {
        if (transaction.TransactionId == Guid.Empty) transaction.TransactionId = Guid.NewGuid();
        transaction.AccountId = accountId;
        transaction.SetAmount();
        transaction.EnteredDate = DateTime.UtcNow;

        await transactionRepo.Insert(transaction);
    }

    public async Task DeleteTransaction(Guid transactionId)
    {
        var transaction = await transactionRepo.Get(transactionId);
        if (transaction.InvoiceId.HasValue)
        {
            var invoice = await context.Invoices.FindAsync(transaction.InvoiceId.Value);
            if (invoice != null)
            {
                await context.Entry(invoice).Collection(i => i.LineItems).LoadAsync();
                await context.Entry(invoice).Reference(i => i.Customer).LoadAsync();
            }
        }

        await transactionRepo.Delete(transactionId);
    }

    public async Task SaveAccountOrder(List<OrderedAccount> accountOrders)
    {
        var accounts = await context.Accounts.ToListAsync();
        foreach (var orderedAccount in accountOrders)
        {
            var account = accounts.FirstOrDefault(a => a.AccountId == orderedAccount.AccountId);
            if (account != null)
            {
                account.DisplayOrder = orderedAccount.DisplayOrder;
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task<TransactionListModel> GetPotentialDuplicateTransactions(Guid accountId)
    {
        var transactions = (await context.Transactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .Where(t => t.AccountId == accountId && t.TransactionDate >= DateTime.UtcNow.AddDays(-60))
                .ToListAsync())
            .GroupBy(t => new { t.TransactionDate, t.Amount })
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .OrderByDescending(t => t.TransactionDate)
            .ThenBy(t => t.Amount)
            .Select(t => t.ToDto())
            .ToList();
        return new TransactionListModel
        {
            Items = transactions,
            TotalCount = transactions.Count,
        };
    }
}