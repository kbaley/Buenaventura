using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]/[action]")]
[ApiController]
public class ReportsController(
    BuenaventuraDbContext context, 
    IReportRepository reportRepo) : ControllerBase
{
    [HttpGet]
    public IActionResult Investment([FromQuery] ReportQuery query )
    {
        var report = new List<dynamic>();

        var date = query.EndDate;
        var numItems = DateTime.Today.Month + 1;
        if (query.SelectedYear != DateTime.Today.Year) {
            numItems = 13;
        }
        for (var i = 0; i < numItems; i++) {
            report.Add(new {date, total=reportRepo.GetInvestmentTotalFor(date)});
            date = date.FirstDayOfMonth().AddMinutes(-1);
        }

        return Ok(new { report, year = query.SelectedYear});
    }

    [HttpGet]
    public IEnumerable<Transaction> ExpensesForCategory(Guid categoryId, DateTime month) {
        var start = new DateTime(month.Year, month.Month, 1);
        var end = start.AddMonths(1);
        var expenses = context.Transactions
            .Include(t => t.Account)
            .Where(t => t.CategoryId == categoryId
                        && t.TransactionDate >= start && t.TransactionDate < end);
        return expenses.ToList();
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardStats()
    {
        var numMonths = 3;
        var gainLossCategory = await context.GetOrCreateCategory("Gain/loss on investments");
        var end = DateTime.Today.LastDayOfMonth();
        var start = end.AddMonths(0 - numMonths).FirstDayOfMonth();
        var investmentGains = await reportRepo.GetMonthlyTotalsForCategory(gainLossCategory.CategoryId, start, end);
        var accountBalances = await context.Accounts
            .Include(a => a.Transactions)
            .Select(a => new {
                a.AccountType,
                Total = a.Transactions.Sum(t => t.Amount)
            }).ToListAsync();
        var netWorthBreakdown = accountBalances
            .Where(a => a.Total != 0)
            .GroupBy(a => a.AccountType, a => a.Total)
            .Select(g => new {
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
        var report = new {
            liquidAssetsBalance,
            creditCardBalance,
            netWorth,
            netWorthLastMonth,
            investmentGains,
            netWorthBreakdown
        };
        return Ok(report);
    }
}

public class NameAndId {
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}