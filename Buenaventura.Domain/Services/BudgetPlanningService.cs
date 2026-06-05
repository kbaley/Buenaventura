using System.Text.RegularExpressions;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IBudgetPlanningService : IAppService
{
    Task<BudgetPlanningSummary> GetBudgetPlanningSummary();
}

public partial class BudgetPlanningService(BuenaventuraDbContext context) : IBudgetPlanningService
{
    private const int MonthsToAnalyze = 12;
    private const int MinimumRecurringOccurrences = 3;
    private const int MinimumRecurringMonths = 3;

    public async Task<BudgetPlanningSummary> GetBudgetPlanningSummary()
    {
        var currentMonthStart = DateTime.Today.FirstDayOfMonth();
        var historyStart = currentMonthStart.AddMonths(-MonthsToAnalyze);
        var historyEnd = currentMonthStart;
        var currentMonthEnd = currentMonthStart.AddMonths(1);

        var historyTransactions = await context.Transactions
            .Include(t => t.Category)
            .Where(t => t.TransactionDate >= historyStart
                        && t.TransactionDate < historyEnd
                        && t.Category != null
                        && t.Category.Type == "Expense")
            .ToListAsync();

        var currentMonthTransactions = await context.Transactions
            .Include(t => t.Category)
            .Where(t => t.TransactionDate >= currentMonthStart
                        && t.TransactionDate < currentMonthEnd
                        && t.Category != null
                        && t.Category.Type == "Expense")
            .ToListAsync();

        var recurringExpenses = DetectRecurringExpenses(historyTransactions);
        var budgetCategories = GenerateBudgetCategories(historyTransactions, currentMonthTransactions, recurringExpenses);

        return new BudgetPlanningSummary
        {
            GeneratedAt = DateTime.UtcNow,
            HistoryStart = historyStart,
            HistoryEnd = historyEnd.AddDays(-1),
            MonthsAnalyzed = MonthsToAnalyze,
            TotalAverageMonthlySpend = budgetCategories.Sum(c => c.AverageMonthlySpend),
            TotalSuggestedMonthlyBudget = budgetCategories.Sum(c => c.SuggestedMonthlyBudget),
            RecurringMonthlyTotal = recurringExpenses.Sum(e => e.AverageAmount),
            RecurringExpenses = recurringExpenses,
            BudgetCategories = budgetCategories
        };
    }

    private static List<RecurringExpenseModel> DetectRecurringExpenses(List<Transaction> transactions)
    {
        return transactions
            .Where(t => t.CategoryId.HasValue && GetExpenseAmount(t) > 0m)
            .Select(t => new RecurringExpenseCandidate(
                NormalizeVendor(t.Vendor, t.Description),
                t.CategoryId!.Value,
                t.Category!.Name,
                GetExpenseAmount(t),
                t.TransactionDate))
            .Where(t => !string.IsNullOrWhiteSpace(t.NormalizedVendor))
            .GroupBy(t => new { t.NormalizedVendor, t.CategoryId })
            .Select(group => BuildRecurringExpense(group.ToList()))
            .Where(t => t is not null)
            .Select(t => t!)
            .OrderByDescending(t => t.AverageAmount)
            .ThenBy(t => t.Vendor)
            .ToList();
    }

    private static RecurringExpenseModel? BuildRecurringExpense(List<RecurringExpenseCandidate> group)
    {
        var transactions = group
            .OrderBy(t => t.TransactionDate)
            .ToList();
        var monthCount = transactions
            .Select(t => new DateTime(t.TransactionDate.Year, t.TransactionDate.Month, 1))
            .Distinct()
            .Count();

        if (transactions.Count < MinimumRecurringOccurrences || monthCount < MinimumRecurringMonths)
        {
            return null;
        }

        var averageAmount = transactions.Average(t => t.Amount);
        var maxAmount = transactions.Max(t => t.Amount);
        var minAmount = transactions.Min(t => t.Amount);
        var amountSpread = averageAmount == 0m ? 0m : (maxAmount - minAmount) / averageAmount;
        var medianGap = GetMedianGapInDays(transactions.Select(t => t.TransactionDate).ToList());
        var looksMonthly = medianGap is >= 25 and <= 35;
        var consistentAmount = amountSpread <= 0.25m;

        if (!looksMonthly && !consistentAmount)
        {
            return null;
        }

        var first = transactions.First();
        var last = transactions.Last();
        var confidence = CalculateConfidence(transactions.Count, monthCount, amountSpread, looksMonthly);

        return new RecurringExpenseModel
        {
            Vendor = ToDisplayVendor(first.NormalizedVendor),
            CategoryId = first.CategoryId,
            CategoryName = first.CategoryName,
            AverageAmount = decimal.Round(averageAmount, 2),
            LastAmount = decimal.Round(last.Amount, 2),
            LastTransactionDate = last.TransactionDate,
            OccurrenceCount = transactions.Count,
            MonthCount = monthCount,
            Cadence = looksMonthly ? "Monthly" : "Recurring",
            Confidence = confidence,
            Explanation = $"{transactions.Count} payments across {monthCount} months, averaging {averageAmount:C0}."
        };
    }

    private static List<BudgetCategoryRecommendation> GenerateBudgetCategories(
        List<Transaction> historyTransactions,
        List<Transaction> currentMonthTransactions,
        List<RecurringExpenseModel> recurringExpenses)
    {
        var recurringByCategory = recurringExpenses
            .GroupBy(e => e.CategoryId)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.AverageAmount));

        return historyTransactions
            .Where(t => t.CategoryId.HasValue)
            .GroupBy(t => new { CategoryId = t.CategoryId!.Value, CategoryName = t.Category!.Name })
            .Select(group =>
            {
                var monthlyAmounts = BuildMonthlyAmounts(group.ToList());
                var average = monthlyAmounts.Average();
                var median = Median(monthlyAmounts);
                var recurringAmount = recurringByCategory.GetValueOrDefault(group.Key.CategoryId);
                var suggestedBudget = RoundBudgetAmount(Math.Max(recurringAmount, Math.Max(average, median)));
                var currentMonthSpend = currentMonthTransactions
                    .Where(t => t.CategoryId == group.Key.CategoryId)
                    .Sum(GetExpenseAmount);

                return new BudgetCategoryRecommendation
                {
                    CategoryId = group.Key.CategoryId,
                    CategoryName = group.Key.CategoryName,
                    AverageMonthlySpend = decimal.Round(average, 2),
                    MedianMonthlySpend = decimal.Round(median, 2),
                    SuggestedMonthlyBudget = suggestedBudget,
                    RecurringMonthlyAmount = decimal.Round(recurringAmount, 2),
                    CurrentMonthSpend = decimal.Round(currentMonthSpend, 2),
                    MonthsWithSpend = monthlyAmounts.Count(amount => amount > 0m),
                    IsCurrentMonthHigherThanBudget = currentMonthSpend > suggestedBudget,
                    Insight = BuildBudgetInsight(group.Key.CategoryName, average, median, recurringAmount, suggestedBudget, currentMonthSpend)
                };
            })
            .OrderByDescending(c => c.SuggestedMonthlyBudget)
            .ThenBy(c => c.CategoryName)
            .ToList();
    }

    private static List<decimal> BuildMonthlyAmounts(List<Transaction> transactions)
    {
        var currentMonthStart = DateTime.Today.FirstDayOfMonth();
        var historyStart = currentMonthStart.AddMonths(-MonthsToAnalyze);
        var monthlyAmounts = new List<decimal>();

        for (var i = 0; i < MonthsToAnalyze; i++)
        {
            var monthStart = historyStart.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            monthlyAmounts.Add(transactions
                .Where(t => t.TransactionDate >= monthStart && t.TransactionDate < monthEnd)
                .Sum(GetExpenseAmount));
        }

        return monthlyAmounts;
    }

    private static string BuildBudgetInsight(
        string categoryName,
        decimal average,
        decimal median,
        decimal recurringAmount,
        decimal suggestedBudget,
        decimal currentMonthSpend)
    {
        if (currentMonthSpend > suggestedBudget)
        {
            return $"{categoryName} is already above the suggested monthly budget.";
        }

        if (recurringAmount > 0m && recurringAmount >= suggestedBudget * 0.75m)
        {
            return $"Most of this budget is recurring spend, so the recommendation stays close to known bills.";
        }

        if (average > median * 1.25m)
        {
            return "A few higher months are pulling the average up, so the recommendation balances average and median spend.";
        }

        return "Based on the last 12 completed months of spending.";
    }

    private static decimal GetExpenseAmount(Transaction transaction)
    {
        return Math.Abs(transaction.AmountInBaseCurrency);
    }

    private static decimal RoundBudgetAmount(decimal amount)
    {
        if (amount <= 0m)
        {
            return 0m;
        }

        return Math.Ceiling(amount / 5m) * 5m;
    }

    private static decimal Median(List<decimal> values)
    {
        var ordered = values.OrderBy(v => v).ToList();
        var middle = ordered.Count / 2;
        return ordered.Count % 2 == 0
            ? (ordered[middle - 1] + ordered[middle]) / 2m
            : ordered[middle];
    }

    private static decimal CalculateConfidence(int occurrenceCount, int monthCount, decimal amountSpread, bool looksMonthly)
    {
        var occurrenceScore = Math.Min(0.35m, occurrenceCount * 0.05m);
        var monthScore = Math.Min(0.35m, monthCount * 0.05m);
        var amountScore = Math.Max(0m, 0.2m - amountSpread);
        var cadenceScore = looksMonthly ? 0.1m : 0m;
        return decimal.Round(Math.Min(0.99m, occurrenceScore + monthScore + amountScore + cadenceScore), 2);
    }

    private static int GetMedianGapInDays(List<DateTime> dates)
    {
        if (dates.Count < 2)
        {
            return 0;
        }

        var gaps = dates
            .Zip(dates.Skip(1), (first, second) => (second - first).Days)
            .OrderBy(gap => gap)
            .ToList();
        return gaps[gaps.Count / 2];
    }

    private static string NormalizeVendor(string? vendor, string? description)
    {
        var value = string.IsNullOrWhiteSpace(vendor) ? description : vendor;
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        var normalized = NonAlphaNumericRegex().Replace(value.Trim().ToUpperInvariant(), " ");
        normalized = MultipleWhitespaceRegex().Replace(normalized, " ").Trim();
        return normalized;
    }

    private static string ToDisplayVendor(string normalizedVendor)
    {
        return string.Join(" ", normalizedVendor
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Length == 1
                ? word
                : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()));
    }

    [GeneratedRegex("[^A-Z0-9]+")]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex("\\s+")]
    private static partial Regex MultipleWhitespaceRegex();

    private sealed record RecurringExpenseCandidate(
        string NormalizedVendor,
        Guid CategoryId,
        string CategoryName,
        decimal Amount,
        DateTime TransactionDate);
}
