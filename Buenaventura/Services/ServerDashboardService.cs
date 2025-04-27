using Buenaventura.Api;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerDashboardService(
    BuenaventuraDbContext context,
    IReportRepository reportRepo) : IDashboardService
{
    public async Task<IEnumerable<ReportDataPoint>> GetNetWorthData()
    {
        var period = ReportPeriod.GetLast12Months();
        var currentDate = period.Start;
        var netWorth = new List<ReportDataPoint>();
        while (currentDate < period.End)
        {
            netWorth.Add(new ReportDataPoint
            {
                Label = currentDate.ToString("MMM yy"),
                Value = await reportRepo.GetNetWorthFor(currentDate.LastDayOfMonth())
            });
            currentDate = currentDate.AddMonths(1);
        }

        return netWorth;
    }

    public async Task<decimal> GetCreditCardBalance()
    {
        var creditCardBalance = await context.Accounts
            .Where(a => a.AccountType == "Credit Card")
            .SumAsync(a => a.Transactions.Sum(t => t.AmountInBaseCurrency));
        return -creditCardBalance;
    }

    public async Task<decimal> GetLiquidAssetBalance()
    {
        var assetBalance = await context.Accounts
            .Where(a => a.AccountType == "Cash" || a.AccountType == "Bank Account")
            .SumAsync(a => a.Transactions.Sum(t => t.AmountInBaseCurrency));
        return assetBalance;
    }

    public async Task<decimal> GetThisMonthExpenses()
    {
        var start = DateTime.Today.FirstDayOfMonth();
        var end = start.AddMonths(1);

        var expenses = await context.Transactions
            .Include(t => t.Category)
            .Where(t => t.TransactionDate >= start && t.TransactionDate < end &&
                        t.Category != null && t.Category.Type == "Expense")
            .SumAsync(t => t.AmountInBaseCurrency);
        return expenses;
    }

    public async Task<IEnumerable<ReportDataPoint>> GetInvestmentData()
    {
        var period = ReportPeriod.GetLast12Months();
        var report = new List<ReportDataPoint>();
        var currentData = period.Start;
        while (currentData < period.End)
        {
            report.Add(new ReportDataPoint
            {
                Label = currentData.ToString("MMM yy"),
                Value = await reportRepo.GetInvestmentTotalFor(currentData.LastDayOfMonth())
            });
            currentData = currentData.AddMonths(1);
        }

        return report;
    }

    public async Task<IEnumerable<ReportDataPoint>> GetExpenseData()
    {
        var period = ReportPeriod.GetLast12Months();
        var expenseData = await GetEntriesByCategoryType("Expense", period.Start, period.End);
        var report = new List<ReportDataPoint>();
        
        foreach (var category in expenseData.Expenses)
        {
            report.Add(new ReportDataPoint
            {
                Label = category.CategoryName,
                Value = category.Total
            });
        }

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

    public async Task<IEnumerable<ReportDataPoint>> GetAssetClassData()
    {
        var assetBalances = (await context.Accounts
                .GroupBy(a => a.AccountType)
                .Select(g => new ReportDataPoint()
                {
                    Label = g.Key,
                    Value = g.Sum(a => a.Transactions.Sum(t => t.AmountInBaseCurrency))
                })
                .ToListAsync())
            .Where(a => a.Value > 0);
        
        return assetBalances;
    }

    public async Task<IEnumerable<ExpenseAveragesDataPoint>> GetExpenseAveragesData()
    {
        // Retrieve expense totals for the last 12 months, grouped by month for the
        // expenses tied to categories where Category.IncludeInReport = true
        var now = DateTime.UtcNow;
        var last30Days = now.AddDays(-30);
        var last90Days = now.AddDays(-90);
        var last360Days = now.AddDays(-360);
        var results = await context.Categories
            .Include(c => c.Transactions)
            .Where(c => c.IncludeInReports)
            .Select(c => new ExpenseAveragesDataPoint
            {
                Category = c.Name,
                Last30Days = c.Transactions
                    .Where(t => t.TransactionDate >= last30Days)
                    .Sum(t => -t.AmountInBaseCurrency),
                Last90DaysAverage = c.Transactions
                    .Where(t => t.TransactionDate >= last90Days)
                    .Sum(t => -t.AmountInBaseCurrency) / 3,
                Last360DaysAverage = c.Transactions
                    .Where(t => t.TransactionDate >= last360Days)
                    .Sum(t => -t.AmountInBaseCurrency) / 12
            }).ToListAsync();
        return results;
    }

    public async Task<IEnumerable<IncomeExpenseDataPoint>> GetIncomeExpenseData()
    {
        // Skip the current month; it'll throw the numbers out of whack because the income is usually at the end
        var period = ReportPeriod.GetLast12MonthsFromLastMonth();

        var incomeExpenseData = new List<IncomeExpenseDataPoint>();
        var currentDate = period.Start;

        while (currentDate < period.End)
        {
            // Technically first day of the next month but we want to include the last day of the current month
            var monthEnd = currentDate.AddMonths(1);

            // Get income from transactions
            var date = currentDate;
            var income = await context.Transactions
                .Include(t => t.Category)
                .Where(t => t.TransactionDate >= date && t.TransactionDate < monthEnd &&
                            t.Category != null && t.Category.Type == "Income")
                .SumAsync(t => t.AmountInBaseCurrency);

            // Get income from invoice line items
            var invoiceIncome = await context.InvoiceLineItems
                .Include(ili => ili.Invoice)
                .Include(ili => ili.Category)
                .Where(ili => ili.Invoice.Date >= date && ili.Invoice.Date < monthEnd &&
                              ili.Category != null && ili.Category.Type == "Income")
                .SumAsync(ili => ili.Quantity * ili.UnitAmount);

            // Get expenses from transactions
            var expenses = await context.Transactions
                .Include(t => t.Category)
                .Where(t => t.TransactionDate >= date && t.TransactionDate < monthEnd &&
                            t.Category != null && t.Category.Type == "Expense")
                .SumAsync(t => t.AmountInBaseCurrency);

            incomeExpenseData.Add(new IncomeExpenseDataPoint
            {
                Date = DateOnly.FromDateTime(currentDate),
                Income = income + invoiceIncome,
                Expenses = -expenses
            });

            currentDate = currentDate.AddMonths(1);
        }

        return incomeExpenseData;
    }

    private async Task<dynamic> GetEntriesByCategoryType(string categoryType, DateTime start, DateTime end)
    {
        var categories = await context.Categories.Where(c => c.Type == categoryType).ToListAsync();
        var expenses = (await reportRepo.GetTransactionsByCategoryType(categoryType, start, end)).ToList();
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
                Amounts = new List<DateAndAmount>()});
        }
        var monthTotals = new List<dynamic>();
        var numMonths = (end.Year * 100 + end.Month) - (start.Year * 100 + start.Month) + 1; 
        for (var i = 0; i < numMonths; i++)
        {
            end = end.FirstDayOfMonth();
            var total = expenses.Sum(e => e.Amounts.Where(x => x.Date == end).Sum(x => x.Amount));
            monthTotals.Add(new { date = end, total });

            end = end.AddMonths(-1);
        }
        return new CategoryTotals{
            Expenses = expenses,
            MonthTotals = monthTotals
        };
    }
    
    private class CategoryTotals
    {
        public IEnumerable<CategoryTotal> Expenses { get; set; } = [];
        public dynamic? MonthTotals { get; set; }
    }
}