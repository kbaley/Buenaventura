using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IExpenseService : IAppService
{
    Task<decimal> GetThisMonthExpenses(Guid? categoryId = null, IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null);
    /// <summary>
    /// Gets a list of expense data points for the last 12 months
    ///
    /// Each data point is a tuple of (category name, total amount)
    /// </summary>
    Task<IEnumerable<ReportDataPoint>> GetExpenseCategoryBreakdown(IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null);
    Task<IEnumerable<ExpenseAveragesDataPoint>> GetExpenseAveragesData(Guid? categoryId = null, IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null);
    /// <summary>
    /// Get a breakdown of expense totals by category and month for the last 12 months
    /// </summary>
    Task<CategoryTotals> GetExpenseTotalsByMonth(IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null);
    /// <summary>
    /// Get a breakdown of expense totals for a category by month for the last 24 months
    /// </summary>
    Task<List<MonthlyAmount>> GetExpenseTotalsByMonth(Guid categoryId, IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null);

    Task<decimal> GetLastMonthExpenses(Guid? categoryId = null, IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null);
    
    /// <summary>
    /// Get spending by vendor for a specific category over the last 12 months
    /// </summary>
    Task<List<ReportDataPoint>> GetVendorSpending(Guid? categoryId = null, IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null);
}

public class ExpenseService(
    BuenaventuraDbContext context,
    IReportRepository reportRepo) : IExpenseService
{
    public async Task<decimal> GetThisMonthExpenses(Guid? categoryId = null, IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null)
    {
        var start = DateTime.Today.FirstDayOfMonth();
        var end = start.AddMonths(1);

        var transactions = await context.Transactions
            .Include(t => t.Category)
            .Where(t => t.TransactionDate >= start && t.TransactionDate < end &&
                        t.Category != null && t.Category.Type == "Expense" &&
                        (categoryId == null || t.CategoryId == categoryId))
            .ToListAsync();
        return FilterByTags(transactions, includeTags, excludeTags).Sum(t => t.AmountInBaseCurrency);
    }

    /// <summary>
    /// Get a breakdown of expense totals by category and month for the last 12 months
    /// </summary>
    public async Task<CategoryTotals> GetExpenseTotalsByMonth(IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null)
    {
        var period = ReportPeriod.GetLast12Months();
        var expenseData = await GetEntriesByCategoryType("Expense", period.Start, period.End, includeTags, excludeTags);
        return expenseData;
    }

    public async Task<List<MonthlyAmount>> GetExpenseTotalsByMonth(Guid categoryId, IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null)
    {
        var period = ReportPeriod.GetLast24Months();
        var transactions = await context.Transactions
            .Where(t => t.TransactionDate > period.Start
                        && t.TransactionDate <= period.End
                        && t.CategoryId == categoryId)
            .ToListAsync();
        var expenses = FilterByTags(transactions, includeTags, excludeTags)
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new MonthlyAmount(g.Key.Year, g.Key.Month, 0 - g.Sum(t => t.AmountInBaseCurrency)))
            .ToList();
        var testDate = period.Start;
        while (testDate < period.End)
        {
            if (!expenses.Any(e => e.Year == testDate.Year && e.Month == testDate.Month))
            {
                expenses.Add(new MonthlyAmount(testDate.Year, testDate.Month, 0.0M));
            }
            testDate = testDate.AddMonths(1);
        }
        expenses = expenses.OrderBy(e => e.Date).ToList();
        
        return expenses;
    }

    public async Task<decimal> GetLastMonthExpenses(Guid? categoryId = null, IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null)
    {
        var start = DateTime.Today.FirstDayOfMonth().AddMonths(-1);
        var end = start.AddMonths(1);

        var transactions = await context.Transactions
            .Include(t => t.Category)
            .Where(t => t.TransactionDate >= start && t.TransactionDate < end &&
                        t.Category != null && t.Category.Type == "Expense" &&
                        (categoryId == null || t.CategoryId == categoryId))
            .ToListAsync();
        return FilterByTags(transactions, includeTags, excludeTags).Sum(t => t.AmountInBaseCurrency);
    }

    /// <summary>
    /// Gets a list of expense data points for the last 12 months
    ///
    /// Each data point is a tuple of (category name, total amount)
    /// </summary>
    public async Task<IEnumerable<ReportDataPoint>> GetExpenseCategoryBreakdown(IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null)
    {
        var period = ReportPeriod.GetLast12Months();
        var expenseData = await GetEntriesByCategoryType("Expense", period.Start, period.End, includeTags, excludeTags);
        var report = expenseData.Expenses
            .Select(category => new ReportDataPoint
            {
                Id = category.CategoryId,
                Label = category.CategoryName,
                Value = category.Total
            }).ToList();

        var otherCategory = new ReportDataPoint
        {
            Label = "Other",
            Value = 0
        };
        var totalExpenses = report.Sum(t => t.Value);
        var threshold = 0.04M * totalExpenses;
        var smallExpenses = report.Where(t => t.Value < threshold);
        otherCategory.Value = smallExpenses.Sum(t => t.Value);
        report.RemoveAll(t => t.Value < threshold);
        report.Add(otherCategory);

        return report;
    }

    public async Task<IEnumerable<ExpenseAveragesDataPoint>> GetExpenseAveragesData(Guid? categoryId = null, IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null)
    {
        // Retrieve expense totals for the last 12 months, grouped by month for the
        // expenses tied to categories where Category.IncludeInReport = true
        var now = DateTime.UtcNow;
        var last30Days = now.AddDays(-30);
        var last90Days = now.AddDays(-90);
        var last360Days = now.AddDays(-360);
        var categories = await context.Categories
            .Include(c => c.Transactions)
            .Where(c => (categoryId.HasValue ? c.CategoryId == categoryId : c.IncludeInReports))
            .ToListAsync();
        var results = categories
            .Select(c =>
            {
                var transactions = FilterByTags(c.Transactions, includeTags, excludeTags).ToList();
                return new ExpenseAveragesDataPoint
                {
                    Category = c.Name,
                    Last30Days = transactions
                        .Where(t => t.TransactionDate >= last30Days)
                        .Sum(t => -t.AmountInBaseCurrency),
                    Last90DaysAverage = transactions
                        .Where(t => t.TransactionDate >= last90Days)
                        .Sum(t => -t.AmountInBaseCurrency) / 3,
                    Last360DaysAverage = transactions
                        .Where(t => t.TransactionDate >= last360Days)
                        .Sum(t => -t.AmountInBaseCurrency) / 12
                };
            }).ToList();
        return results;
    }

    public async Task<List<ReportDataPoint>> GetVendorSpending(Guid? categoryId = null, IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null)
    {
        var startDate = DateTime.Today.AddYears(-1);
        
        // Get vendor spending for the last year
        var transactions = await context.Transactions
            .Where(t => (categoryId == null || t.CategoryId == categoryId)
                        && t.TransactionDate >= startDate 
                        && !string.IsNullOrEmpty(t.Vendor))
            .ToListAsync();
        var vendorSpending = FilterByTags(transactions, includeTags, excludeTags)
            .GroupBy(t => t.Vendor)
            .Select(g => new ReportDataPoint
            {
                Label = g.Key!,
                Value = -g.Sum(t => t.AmountInBaseCurrency)
            })
            .OrderByDescending(v => v.Value)
            .ToList();

        if (!vendorSpending.Any())
        {
            return [];
        }

        // Calculate total spending
        var totalSpending = vendorSpending.Sum(v => v.Value);
        
        // Group vendors with less than 2% into "Other"
        var threshold = totalSpending * 0.02m;
        var mainVendors = vendorSpending.Where(v => v.Value >= threshold).ToList();
        var smallVendors = vendorSpending.Where(v => v.Value < threshold).ToList();
        
        if (smallVendors.Any())
        {
            var otherTotal = smallVendors.Sum(v => v.Value);
            mainVendors.Add(new ReportDataPoint
            {
                Label = "Other",
                Value = otherTotal
            });
        }
        
        return mainVendors;
    }

    private async Task<CategoryTotals> GetEntriesByCategoryType(string categoryType, DateTime start, DateTime end, IEnumerable<string>? includeTags = null, IEnumerable<string>? excludeTags = null)
    {
        var categories = await context.Categories.Where(c => c.Type == categoryType).ToListAsync();
        var transactions = await context.Transactions
            .Include(t => t.Category)
            .Where(t => t.TransactionDate > start
                        && t.TransactionDate <= end
                        && t.Category != null
                        && t.Category.Type == categoryType)
            .ToListAsync();
        var amountMultiplier = categoryType == "Expense" ? -1 : 1;
        var expenses = FilterByTags(transactions, includeTags, excludeTags)
            .GroupBy(t => new { t.CategoryId, CategoryName = t.Category!.Name })
            .Select(g => new CategoryTotal
            {
                CategoryId = g.Key.CategoryId!.Value,
                CategoryName = g.Key.CategoryName,
                Amounts = g
                    .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                    .Select(month => new MonthlyAmount(
                        month.Key.Year,
                        month.Key.Month,
                        amountMultiplier * month.Sum(t => t.AmountInBaseCurrency)))
                    .ToList()
            })
            .ToList();
        if (categoryType == "Income") {
            var invoiceTotals = reportRepo.GetInvoiceLineItemsIncomeTotals(start, end);
            foreach (var item in invoiceTotals)
            {
                var match = expenses.SingleOrDefault(e => e.CategoryId == item.CategoryId);
                if (match == null) {
                    expenses.Add(item);
                } else {
                    match.Merge(item);
                }
            }
        }
        expenses.ForEach(e => e.Total = e.Amounts.Sum(a => a.Amount));
        expenses = expenses.OrderByDescending(e => e.Total).ToList();

        // Add categories with no expenses
        var missingCategories = categories.Where(c => expenses.All(e => e.CategoryId != c.CategoryId)).ToList();
        foreach (var category in missingCategories)
        {
            expenses.Add(new CategoryTotal{ 
                CategoryId = category.CategoryId, 
                CategoryName = category.Name, 
                Total = 0.0M,
                Amounts = new List<MonthlyAmount>()});
        }
        var monthTotals = new List<MonthlyAmount>();
        var numMonths = end.Year * 12 + end.Month - (start.Year * 12 + start.Month);
        if (end.Date > DateTime.Today)
        {
            end = end.AddMonths(-1);
        }
        for (var i = 0; i < numMonths; i++)
        {
            end = end.FirstDayOfMonth();
            var total = expenses.Sum(e => e.Amounts.Where(x => x.Date == end).Sum(x => x.Amount));
            monthTotals.Add(new MonthlyAmount(end.Year, end.Month, total));

            end = end.AddMonths(-1);
        }
        return new CategoryTotals{
            Expenses = expenses,
            MonthTotals = monthTotals
        };
    }

    private static IEnumerable<Transaction> FilterByTags(IEnumerable<Transaction> transactions, IEnumerable<string>? includeTags, IEnumerable<string>? excludeTags)
    {
        return transactions.Where(t => TransactionTagFormatter.Matches(t.Tags, includeTags, excludeTags));
    }
}
