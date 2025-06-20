﻿using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]/[action]")]
[ApiController]
public class DashboardController(
    BuenaventuraDbContext context, 
    IDashboardService dashboardService,
    IReportRepository reportRepo) : ControllerBase
{
    [HttpGet]
    public async Task<decimal> CreditCardBalance()
    {
        return await dashboardService.GetCreditCardBalance();        
    }
    
    [HttpGet]
    public async Task<decimal> LiquidAssetBalance()
    {
        return await dashboardService.GetLiquidAssetBalance();        
    }
    
    [HttpGet]
    public async Task<decimal> ExpensesThisMonth()
    {
        return await dashboardService.GetThisMonthExpenses();        
    }

    [HttpGet]
    public async Task<IEnumerable<ReportDataPoint>> NetWorth()
    {
        return await dashboardService.GetNetWorthData();
    }

    [HttpGet]
    public async Task<IEnumerable<ReportDataPoint>> Investments()
    {
        return await dashboardService.GetInvestmentData();
    }

    [HttpGet]
    public async Task<IEnumerable<ReportDataPoint>> Expenses()
    {
        return await dashboardService.GetExpenseCategoryBreakdown();
    }
    
    [HttpGet]
    [ActionName("expensetotalsbymonth")]
    public async Task<CategoryTotals> GetExpenseTotalsByMonth()
    {
        return await dashboardService.GetExpenseTotalsByMonth();
    }

    [HttpGet]
    public async Task<IEnumerable<ReportDataPoint>> AssetClasses()
    {
        return await dashboardService.GetAssetClassData();
    }

    [HttpGet]
    public async Task<IEnumerable<ExpenseAveragesDataPoint>> ExpenseAverages()
    {
        return await dashboardService.GetExpenseAveragesData();
    }
    
    [HttpGet]
    public async Task<IEnumerable<IncomeExpenseDataPoint>> IncomeExpenses()
    {
        return await dashboardService.GetIncomeExpenseData();        
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
