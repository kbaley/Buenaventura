using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IDashboardService : IAppService
{
    Task<IEnumerable<ReportDataPoint>> GetNetWorthData();
    Task<decimal> GetCreditCardBalance();
    Task<decimal> GetLiquidAssetBalance();
    Task<IEnumerable<IncomeExpenseDataPoint>> GetIncomeExpenseData();
    Task<IEnumerable<ReportDataPoint>> GetInvestmentData();
    Task<IEnumerable<ReportDataPoint>> GetAssetClassData();
}

public class DashboardService(
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

    public async Task<IEnumerable<ReportDataPoint>> GetInvestmentData()
    {
        var period = ReportPeriod.GetLast12Months();
        var report = new List<ReportDataPoint>();
        var currentData = period.Start;
        var cumulativeValue = 0m;
        while (currentData < period.End)
        {
            var startDate = currentData.FirstDayOfMonth();
            var endDate = currentData.LastDayOfMonth();
            var reportValue = await reportRepo.GetInvestmentChangeFor(startDate, endDate);
            cumulativeValue += reportValue;
            report.Add(new ReportDataPoint
            {
                Label = currentData.ToString("MMM yy"),
                Value = cumulativeValue
            });
            currentData = currentData.AddMonths(1);
        }

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
}