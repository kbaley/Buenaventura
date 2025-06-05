using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IDashboardService : IAppService
{
    Task<IEnumerable<ReportDataPoint>> GetNetWorthData();
    Task<decimal> GetCreditCardBalance();
    Task<decimal> GetLiquidAssetBalance();
    Task<IEnumerable<IncomeExpenseDataPoint>> GetIncomeExpenseData();
    Task<decimal> GetThisMonthExpenses();
    Task<IEnumerable<ReportDataPoint>> GetInvestmentData();
    /// <summary>
    /// Gets a list of expense data points for the last 12 months
    ///
    /// Each data point is a tuple of (category name, total amount)
    /// </summary>
    Task<IEnumerable<ReportDataPoint>> GetExpenseCategoryBreakdown();
    Task<IEnumerable<ReportDataPoint>> GetAssetClassData();
    Task<IEnumerable<ExpenseAveragesDataPoint>> GetExpenseAveragesData();
    /// <summary>
    /// Get a breakdown of expense totals by category and month for the last 12 months
    /// </summary>
    Task<CategoryTotals> GetExpenseTotalsByMonth();
}