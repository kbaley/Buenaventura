using AutoMapper;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Dtos;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerAccountService(
    IDbContextFactory<CoronadoDbContext> dbContextFactory,
    ITransactionRepository transactionRepo,
    IMapper mapper
) : IAccountService
{
    public async Task<IEnumerable<AccountWithBalance>> GetAccounts()
    {
        var context = await dbContextFactory.CreateDbContextAsync();
        var exchangeRate = await context.Currencies.GetCadExchangeRate();
        var accounts = await context.Accounts
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
        var context = await dbContextFactory.CreateDbContextAsync();
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
        var context = await dbContextFactory.CreateDbContextAsync();
        if (transaction.TransactionId == Guid.Empty) transaction.TransactionId = Guid.NewGuid();
        transaction.AccountId = accountId;
        transaction.SetAmount();
        transaction.EnteredDate = DateTime.UtcNow;
        if (transaction.CategoryId.IsNullOrEmpty() && !string.IsNullOrWhiteSpace(transaction.CategoryDisplay))
        {
            transaction.CategoryId = (await context.GetOrCreateCategory(transaction.CategoryDisplay)).CategoryId;
        }

        await transactionRepo.Insert(transaction);
    }

    public async Task DeleteTransaction(Guid transactionId)
    {
        var context = await dbContextFactory.CreateDbContextAsync();
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
}