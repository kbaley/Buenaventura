using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IDashboardApi
{
    
    [Get("/api/dashboard/networth")]
    Task<IEnumerable<ReportDataPoint>> GetNetWorthData();
    
    [Get("/api/dashboard/creditcardbalance")]
    Task<decimal> GetCreditCardBalance();
    
    [Get("/api/dashboard/liquidassetbalance")]
    Task<decimal> GetLiquidAssetBalance();
    
    [Get("/api/dashboard/incomeexpenses")]
    Task<IEnumerable<IncomeExpenseDataPoint>> GetIncomeExpenseData();
    
    [Get("/api/dashboard/expensesthismonth")]
    Task<decimal> GetThisMonthExpenses();
    
    [Get("/api/dashboard/investments")]
    Task<IEnumerable<ReportDataPoint>> GetInvestmentData();
    
    /// <summary>
    /// Gets a list of expense data points for the last 12 months
    ///
    /// Each data point is a tuple of (category name, total amount)
    /// </summary>
    [Get("/api/dashboard/expenses")]
    Task<IEnumerable<ReportDataPoint>> GetExpenseCategoryBreakdown();
    
    [Get("/api/dashboard/assetclasses")]
    Task<IEnumerable<ReportDataPoint>> GetAssetClassData();
    
    [Get("/api/dashboard/expenseaverages")]
    Task<IEnumerable<ExpenseAveragesDataPoint>> GetExpenseAveragesData();
    
    /// <summary>
    /// Get a breakdown of expense totals by category and month for the last 12 months
    /// </summary>
    [Get("/api/dashboard/expensetotalsbymonth")]
    Task<CategoryTotals> GetExpenseTotalsByMonth();
}