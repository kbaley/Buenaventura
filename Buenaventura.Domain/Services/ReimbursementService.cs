using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IReimbursementService : IAppService
{
    Task<ReimbursementSummary> GetSummary();
    Task<ReimbursementReport> GetReport();
    Task CreateSettlement(CreateReimbursementSettlementRequest request);
}

public class ReimbursementService(BuenaventuraDbContext context) : IReimbursementService
{
    private const string ReimbursementCategoryName = "To be reimbursed";

    public async Task<ReimbursementSummary> GetSummary()
    {
        var report = await GetReport();
        return report.Summary;
    }

    public async Task<ReimbursementReport> GetReport()
    {
        var transactions = await GetReimbursementTransactions();
        var ledger = BuildLedger(transactions);
        var monthlyRows = BuildMonthlyRows(ledger);
        var unsettledLedger = ledger
            .Where(t => !t.IsSettlementClosed)
            .ToList();
        var unsettledExpenses = unsettledLedger
            .Where(t => t.ReimbursementAmount > 0)
            .OrderByDescending(t => t.TransactionDate)
            .ToList();
        var unsettledRepayments = unsettledLedger
            .Where(t => t.ReimbursementAmount < 0)
            .OrderByDescending(t => t.TransactionDate)
            .ToList();
        var summary = BuildSummary(ledger, unsettledExpenses, unsettledRepayments);

        return new ReimbursementReport
        {
            Summary = summary,
            MonthlyRows = monthlyRows,
            UnsettledExpenses = unsettledExpenses,
            UnsettledRepayments = unsettledRepayments,
            Settlements = await GetSettlements(),
            RecentTransactions = ledger
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.RunningBalance)
                .Take(100)
                .ToList()
        };
    }

    public async Task CreateSettlement(CreateReimbursementSettlementRequest request)
    {
        var transactionIds = request.TransactionIds
            .Concat(request.Matches.SelectMany(m => m.TransactionIds))
            .Distinct()
            .ToList();
        if (transactionIds.Count == 0)
        {
            return;
        }

        var transactions = await context.Transactions
            .Include(t => t.Category)
            .Where(t => transactionIds.Contains(t.TransactionId))
            .Where(t => t.Category != null && t.Category.Name.ToLower() == ReimbursementCategoryName.ToLower())
            .ToListAsync();
        if (transactions.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var settlement = new ReimbursementSettlement
        {
            ReimbursementSettlementId = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(request.Name)
                ? $"Reimbursement settlement - {DateTime.Today:MMM yyyy}"
                : request.Name.Trim(),
            Notes = request.Notes.Trim(),
            CreatedDate = now,
            ClosedDate = request.CloseImmediately ? now : null
        };

        context.ReimbursementSettlements.Add(settlement);
        foreach (var transaction in transactions)
        {
            transaction.ReimbursementSettlementId = settlement.ReimbursementSettlementId;
        }

        var matches = request.Matches
            .Where(m => m.TransactionIds.Any())
            .ToList();
        if (matches.Count == 0)
        {
            matches.Add(new CreateReimbursementMatchRequest
            {
                Notes = "Settled together",
                TransactionIds = transactions.Select(t => t.TransactionId).ToList()
            });
        }

        foreach (var matchRequest in matches)
        {
            var matchTransactionIds = matchRequest.TransactionIds
                .Where(id => transactions.Any(t => t.TransactionId == id))
                .Distinct()
                .ToList();
            if (matchTransactionIds.Count == 0)
            {
                continue;
            }

            var match = new ReimbursementMatch
            {
                ReimbursementMatchId = Guid.NewGuid(),
                ReimbursementSettlementId = settlement.ReimbursementSettlementId,
                Notes = matchRequest.Notes.Trim(),
                AcceptedDifferenceReason = matchRequest.AcceptedDifferenceReason.Trim(),
                MatchTransactions = matchTransactionIds
                    .Select(id => new ReimbursementMatchTransaction
                    {
                        ReimbursementMatchId = settlement.ReimbursementSettlementId,
                        TransactionId = id
                    })
                    .ToList()
            };
            foreach (var matchTransaction in match.MatchTransactions)
            {
                matchTransaction.ReimbursementMatchId = match.ReimbursementMatchId;
            }

            context.ReimbursementMatches.Add(match);
        }

        await context.SaveChangesAsync();
    }

    private async Task<List<ReimbursementTransactionSource>> GetReimbursementTransactions()
    {
        return await context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.Category != null && t.Category.Name.ToLower() == ReimbursementCategoryName.ToLower())
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.EnteredDate)
            .ThenBy(t => t.TransactionId)
            .Select(t => new ReimbursementTransactionSource
            {
                TransactionDate = t.TransactionDate,
                TransactionId = t.TransactionId,
                ReimbursementSettlementId = t.ReimbursementSettlementId,
                SettlementName = t.ReimbursementSettlement == null ? "" : t.ReimbursementSettlement.Name,
                IsSettlementClosed = t.ReimbursementSettlement != null && t.ReimbursementSettlement.ClosedDate != null,
                AccountName = t.Account == null ? "" : t.Account.Name,
                Vendor = t.Vendor ?? "",
                Description = t.Description ?? "",
                Amount = t.AmountInBaseCurrency
            })
            .ToListAsync();
    }

    private async Task<List<ReimbursementSettlementModel>> GetSettlements()
    {
        var settlements = await context.ReimbursementSettlements
            .Include(s => s.Transactions)
            .Include(s => s.Matches)
            .OrderByDescending(s => s.CreatedDate)
            .Take(50)
            .Select(s => new ReimbursementSettlementModel
            {
                ReimbursementSettlementId = s.ReimbursementSettlementId,
                Name = s.Name,
                Notes = s.Notes,
                CreatedDate = s.CreatedDate,
                ClosedDate = s.ClosedDate,
                Expenses = s.Transactions
                    .Where(t => t.AmountInBaseCurrency < 0)
                    .Sum(t => -t.AmountInBaseCurrency),
                Repayments = s.Transactions
                    .Where(t => t.AmountInBaseCurrency > 0)
                    .Sum(t => t.AmountInBaseCurrency),
                Difference = s.Transactions.Sum(t => -t.AmountInBaseCurrency),
                TransactionCount = s.Transactions.Count,
                MatchCount = s.Matches.Count
            })
            .ToListAsync();

        return settlements;
    }

    private static List<ReimbursementTransaction> BuildLedger(IEnumerable<ReimbursementTransactionSource> transactions)
    {
        var runningBalance = 0m;
        var ledger = new List<ReimbursementTransaction>();

        foreach (var transaction in transactions)
        {
            var reimbursementAmount = -transaction.Amount;
            runningBalance += reimbursementAmount;
            ledger.Add(new ReimbursementTransaction
            {
                TransactionId = transaction.TransactionId,
                ReimbursementSettlementId = transaction.ReimbursementSettlementId,
                SettlementName = transaction.SettlementName,
                IsSettlementClosed = transaction.IsSettlementClosed,
                TransactionDate = transaction.TransactionDate,
                AccountName = transaction.AccountName,
                Vendor = transaction.Vendor,
                Description = transaction.Description,
                Amount = transaction.Amount,
                ReimbursementAmount = reimbursementAmount,
                RunningBalance = runningBalance
            });
        }

        return ledger;
    }

    private static List<ReimbursementMonth> BuildMonthlyRows(List<ReimbursementTransaction> ledger)
    {
        if (ledger.Count == 0)
        {
            return [];
        }

        var firstMonth = new DateTime(ledger.Min(t => t.TransactionDate).Year, ledger.Min(t => t.TransactionDate).Month, 1);
        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var runningBalance = 0m;
        var rows = new List<ReimbursementMonth>();

        for (var month = firstMonth; month <= currentMonth; month = month.AddMonths(1))
        {
            var monthTransactions = ledger
                .Where(t => t.TransactionDate.Year == month.Year && t.TransactionDate.Month == month.Month)
                .ToList();
            var expenses = monthTransactions
                .Where(t => t.ReimbursementAmount > 0)
                .Sum(t => t.ReimbursementAmount);
            var repayments = monthTransactions
                .Where(t => t.ReimbursementAmount < 0)
                .Sum(t => -t.ReimbursementAmount);
            var net = expenses - repayments;
            runningBalance += net;

            rows.Add(new ReimbursementMonth
            {
                Month = month,
                Expenses = expenses,
                Repayments = repayments,
                Net = net,
                RunningBalance = runningBalance
            });
        }

        return rows
            .OrderByDescending(r => r.Month)
            .Take(24)
            .ToList();
    }

    private static ReimbursementSummary BuildSummary(
        List<ReimbursementTransaction> ledger,
        List<ReimbursementTransaction> unsettledExpenses,
        List<ReimbursementTransaction> unsettledRepayments)
    {
        if (ledger.Count == 0)
        {
            return new ReimbursementSummary();
        }

        var lastYear = DateTime.Today.AddMonths(-12);
        var outstandingBalance = unsettledExpenses.Sum(t => t.ReimbursementAmount)
                                 + unsettledRepayments.Sum(t => t.ReimbursementAmount);
        DateTime? oldestOutstandingDate = unsettledExpenses.Count == 0
            ? null
            : unsettledExpenses.Min(t => t.TransactionDate);

        return new ReimbursementSummary
        {
            HasTransactions = true,
            OutstandingBalance = outstandingBalance,
            ExpensesLast12Months = ledger
                .Where(t => t.TransactionDate >= lastYear && t.ReimbursementAmount > 0)
                .Sum(t => t.ReimbursementAmount),
            RepaymentsLast12Months = ledger
                .Where(t => t.TransactionDate >= lastYear && t.ReimbursementAmount < 0)
                .Sum(t => -t.ReimbursementAmount),
            NetLast12Months = ledger
                .Where(t => t.TransactionDate >= lastYear)
                .Sum(t => t.ReimbursementAmount),
            OldestOutstandingDate = oldestOutstandingDate,
            OldestOutstandingDays = oldestOutstandingDate.HasValue
                ? (DateTime.Today - oldestOutstandingDate.Value.Date).Days
                : null,
            UnsettledExpenseCount = unsettledExpenses.Count,
            UnsettledExpenseTotal = unsettledExpenses.Sum(t => t.ReimbursementAmount),
            UnsettledRepaymentCount = unsettledRepayments.Count,
            UnsettledRepaymentTotal = unsettledRepayments.Sum(t => -t.ReimbursementAmount),
            LastExpenseDate = ledger
                .Where(t => t.ReimbursementAmount > 0)
                .Select(t => (DateTime?)t.TransactionDate)
                .LastOrDefault(),
            LastRepaymentDate = ledger
                .Where(t => t.ReimbursementAmount < 0)
                .Select(t => (DateTime?)t.TransactionDate)
                .LastOrDefault()
        };
    }

    private sealed record ReimbursementTransactionSource
    {
        public Guid TransactionId { get; init; }
        public Guid? ReimbursementSettlementId { get; init; }
        public string SettlementName { get; init; } = "";
        public bool IsSettlementClosed { get; init; }
        public DateTime TransactionDate { get; init; }
        public string AccountName { get; init; } = "";
        public string Vendor { get; init; } = "";
        public string Description { get; init; } = "";
        public decimal Amount { get; init; }
    }
}
