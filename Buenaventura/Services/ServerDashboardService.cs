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
        if (year != DateTime.Today.Year) {
            numItems = 13;
        }
        for (var i = 0; i < numItems; i++) {
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
    
    public async Task<IEnumerable<IncomeExpenseDataPoint>> GetIncomeExpenseData(int? year = null)
    {
        year ??= DateTime.Today.Year;
        var context = await dbContextFactory.CreateDbContextAsync();
        var start = new DateTime(year.Value, 1, 1);
        var end = new DateTime(year.Value, 12, 31);
        
    }
}