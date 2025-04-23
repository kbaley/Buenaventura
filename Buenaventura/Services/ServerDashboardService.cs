using Buenaventura.Api;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerDashboardService(
    IDbContextFactory<BuenaventuraDbContext> dbContextFactory,
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
        var context = await dbContextFactory.CreateDbContextAsync();
        var creditCardBalance = context.Accounts
            .Where(a => a.AccountType == "Credit Card")
            .Sum(a => a.Transactions.Sum(t => t.AmountInBaseCurrency));
        return -creditCardBalance;
    }

    public async Task<decimal> GetLiquidAssetBalance()
    {
        var context = await dbContextFactory.CreateDbContextAsync();
        var assetBalance = context.Accounts
            .Where(a => a.AccountType == "Cash" || a.AccountType == "Bank Account")
            .Sum(a => a.Transactions.Sum(t => t.AmountInBaseCurrency));
        return assetBalance;
    }

    public async Task<decimal> GetThisMonthExpenses()
    {
        var start = DateTime.Today.FirstDayOfMonth();
        var end = start.AddMonths(1);

        var context = await dbContextFactory.CreateDbContextAsync();
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

    public async Task<IEnumerable<IncomeExpenseDataPoint>> GetIncomeExpenseData()
    {
        // Skip the current month; it'll throw the numbers out of whack because the income is usually at the end
        var period = ReportPeriod.GetLast12MonthsFromLastMonth();

        var context = await dbContextFactory.CreateDbContextAsync();

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
}

/// <summary>
/// Dates for a reporting period
///
/// The end date is meant to be exclusive, so the period is [start, end)
/// </summary>
public class ReportPeriod
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    
    public static ReportPeriod GetLast12Months()
    {
        var today = DateTime.Today;
        var end = today.FirstDayOfMonth().AddMonths(1);
        var start = today.AddMonths(-11).FirstDayOfMonth();
        return new ReportPeriod
        {
            Start = start,
            End = end
        };
    }
    
    /// <summary>
    /// Get the 12-month period ending the first day of this month
    /// </summary>
    /// <returns></returns>
    public static ReportPeriod GetLast12MonthsFromLastMonth()
    {
        var today = DateTime.Today;
        var end = today.FirstDayOfMonth();
        var start = today.AddMonths(-12).FirstDayOfMonth();
        return new ReportPeriod
        {
            Start = start,
            End = end
        };
    }
}