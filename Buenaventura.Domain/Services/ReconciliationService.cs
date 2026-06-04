using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IReconciliationService : IAppService
{
    Task<ReconciliationWorkspace> GetWorkspace(Guid accountId, DateTime asOfDate);
    Task Complete(CompleteReconciliationRequest request);
}

public class ReconciliationService(BuenaventuraDbContext context) : IReconciliationService
{
    public async Task<ReconciliationWorkspace> GetWorkspace(Guid accountId, DateTime asOfDate)
    {
        var account = await context.Accounts
            .Include(a => a.Transactions)
            .SingleOrDefaultAsync(a => a.AccountId == accountId);

        if (account == null)
        {
            return new ReconciliationWorkspace();
        }

        var transactions = await context.Transactions
            .Include(t => t.LeftTransfer)
            .Include(t => t.LeftTransfer!.RightTransaction)
            .Include(t => t.LeftTransfer!.RightTransaction!.Account)
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.AccountId == accountId
                        && !t.IsReconciled
                        && t.TransactionDate <= asOfDate.Date)
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.EnteredDate)
            .ThenBy(t => t.TransactionId)
            .Select(t => t.ToDto())
            .ToListAsync();

        transactions.ForEach(t => t.SetDebitAndCredit());

        return new ReconciliationWorkspace
        {
            Account = new AccountWithBalance
            {
                AccountId = account.AccountId,
                Name = account.Name,
                Currency = account.Currency,
                Vendor = account.Vendor,
                AccountType = account.AccountType,
                MortgagePayment = account.MortgagePayment,
                MortgageType = account.MortgageType,
                DisplayOrder = account.DisplayOrder,
                IsHidden = account.IsHidden,
                ExcludeFromReports = account.ExcludeFromReports,
                CurrentBalance = account.Transactions.Sum(t => t.Amount),
            },
            AsOfDate = asOfDate.Date,
            CurrentBalance = account.Transactions.Sum(t => t.Amount),
            ReconciledBalance = account.Transactions
                .Where(t => t.IsReconciled)
                .Sum(t => t.Amount),
            Transactions = transactions
        };
    }

    public async Task Complete(CompleteReconciliationRequest request)
    {
        if (request.TransactionIds.Count == 0)
        {
            return;
        }

        var transactions = await context.Transactions
            .Where(t => t.AccountId == request.AccountId
                        && request.TransactionIds.Contains(t.TransactionId))
            .ToListAsync();

        foreach (var transaction in transactions)
        {
            transaction.IsReconciled = true;
        }

        await context.SaveChangesAsync();
    }
}
