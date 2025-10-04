using Buenaventura.Data;
using Buenaventura.Domain;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

internal class GetDashboardStats(
    BuenaventuraDbContext context,
    IReportRepository reportRepo)
    : EndpointWithoutRequest<object>
{
    public override void Configure()
    {
        Get("/api/dashboard/getdashboardstats");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var numMonths = 3;
        var gainLossCategory = await context.GetOrCreateCategory("Gain/loss on investments");
        var end = DateTime.Today.LastDayOfMonth();
        var start = end.AddMonths(0 - numMonths).FirstDayOfMonth();
        var investmentGains = await reportRepo.GetMonthlyTotalsForCategory(gainLossCategory.CategoryId, start, end);

        var accountBalances = await context.Accounts
            .Include(a => a.Transactions)
            .Select(a => new
            {
                a.AccountType,
                Total = a.Transactions.Sum(t => t.Amount)
            }).ToListAsync(ct);

        var netWorthBreakdown = accountBalances
            .Where(a => a.Total != 0)
            .GroupBy(a => a.AccountType, a => a.Total)
            .Select(g => new
            {
                AccountType = g.Key,
                Total = g.Sum()
            })
            .OrderByDescending(a => a.Total)
            .ToList();

        var liquidAssetsBalance = context.Accounts
            .Where(a => a.AccountType == "Bank Account" || a.AccountType == "Cash")
            .Sum(a => a.Transactions.Sum(t => t.AmountInBaseCurrency));
        var creditCardBalance = context.Accounts
            .Where(a => a.AccountType == "Credit Card")
            .Sum(a => a.Transactions.Sum(t => t.AmountInBaseCurrency));
        var netWorth = context.Transactions.Sum(t => t.AmountInBaseCurrency);

        var firstdayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var netWorthLastMonth = context.Transactions
            .Where(t => t.TransactionDate < firstdayOfMonth)
            .Sum(t => t.AmountInBaseCurrency);

        var report = new
        {
            liquidAssetsBalance,
            creditCardBalance,
            netWorth,
            netWorthLastMonth,
            investmentGains,
            netWorthBreakdown
        };

        await SendOkAsync(report, ct);
    }
}