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
    public async Task<IEnumerable<ReportDataPoint>> GetNetWorthData(int? year = null)
    {
        year ??= DateTime.Today.Year;
        var netWorth = new List<ReportDataPoint>();

        var date = year.Value.GetEndDateForYear();
        var numItems = DateTime.Today.Month + 1;
        if (year != DateTime.Today.Year)
        {
            numItems = 13;
        }

        for (var i = 0; i < numItems; i++)
        {
            netWorth.Add(new ReportDataPoint
            {
                Label = date.ToString("MMM yy"),
                Value = await reportRepo.GetNetWorthFor(date)
            });
            date = date.FirstDayOfMonth().AddMinutes(-1);
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

    public async Task<IEnumerable<IncomeExpenseDataPoint>> GetIncomeExpenseData()
    {
        // Skip the current month; it'll throw the numbers out of whack because the income is usually at the end
        var today = DateTime.Today.AddMonths(-1);
        var endDate = today.AddDays(1);
        var startDate = today.AddMonths(-11).FirstDayOfMonth(); // Go back 11 months to get 12 months total

        var context = await dbContextFactory.CreateDbContextAsync();

        var incomeExpenseData = new List<IncomeExpenseDataPoint>();
        var currentDate = startDate;

        while (currentDate <= endDate)
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