using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IReimbursementService : IAppService
{
    Task<ReimbursementSummary> GetSummary();
    Task<ReimbursementReport> GetReport();
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
        var summary = BuildSummary(ledger);

        return new ReimbursementReport
        {
            Summary = summary,
            MonthlyRows = monthlyRows,
            RecentTransactions = ledger
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.RunningBalance)
                .Take(100)
                .ToList()
        };
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
                AccountName = t.Account == null ? "" : t.Account.Name,
                Vendor = t.Vendor ?? "",
                Description = t.Description ?? "",
                Amount = t.AmountInBaseCurrency
            })
            .ToListAsync();
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

    private static ReimbursementSummary BuildSummary(List<ReimbursementTransaction> ledger)
    {
        if (ledger.Count == 0)
        {
            return new ReimbursementSummary();
        }

        var lastYear = DateTime.Today.AddMonths(-12);
        var outstandingExpenses = GetOutstandingExpenses(ledger);
        var outstandingBalance = ledger.Last().RunningBalance;
        var oldestOutstandingDate = outstandingExpenses.FirstOrDefault()?.TransactionDate;

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

    private static List<OutstandingExpense> GetOutstandingExpenses(IEnumerable<ReimbursementTransaction> ledger)
    {
        var expenses = new Queue<OutstandingExpense>();
        var surplusRepayment = 0m;

        foreach (var transaction in ledger)
        {
            if (transaction.ReimbursementAmount > 0)
            {
                if (surplusRepayment >= transaction.ReimbursementAmount)
                {
                    surplusRepayment -= transaction.ReimbursementAmount;
                    continue;
                }

                var outstandingAmount = transaction.ReimbursementAmount - surplusRepayment;
                surplusRepayment = 0;
                expenses.Enqueue(new OutstandingExpense(transaction.TransactionDate, outstandingAmount));
                continue;
            }

            var repaymentRemaining = -transaction.ReimbursementAmount;
            while (repaymentRemaining > 0 && expenses.Count > 0)
            {
                var expense = expenses.Dequeue();
                if (expense.Amount <= repaymentRemaining)
                {
                    repaymentRemaining -= expense.Amount;
                    continue;
                }

                expenses.Enqueue(expense with { Amount = expense.Amount - repaymentRemaining });
                repaymentRemaining = 0;
            }

            surplusRepayment += repaymentRemaining;
        }

        return expenses.ToList();
    }

    private sealed record ReimbursementTransactionSource
    {
        public DateTime TransactionDate { get; init; }
        public string AccountName { get; init; } = "";
        public string Vendor { get; init; } = "";
        public string Description { get; init; } = "";
        public decimal Amount { get; init; }
    }

    private sealed record OutstandingExpense(DateTime TransactionDate, decimal Amount);
}
