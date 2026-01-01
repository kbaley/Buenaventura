using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IExpensesApi
{
    /// <summary>
    /// Gets a list of expense data points for the last 12 months
    ///
    /// Each data point is a tuple of (category name, total amount)
    /// </summary>
    [Get("/api/expenses")]
    Task<IEnumerable<ReportDataPoint>> GetExpenseCategoryBreakdown();
    
    [Get("/api/expenses/averages")]
    Task<IEnumerable<ExpenseAveragesDataPoint>> GetExpenseAveragesData();
    
    /// <summary>
    /// Get a breakdown of expense totals by category and month for the last 12 months
    /// </summary>
    [Get("/api/expenses/totalsbymonth")]
    Task<CategoryTotals> GetExpenseTotalsByMonth();
    
    [Get("/api/expenses/thismonth")]
    Task<decimal> GetThisMonthExpenses();

    [Get("/api/expenses/category/{categoryId}")]
    Task<ExpenseCategoryPageData> GetExpenseCategoryPageData(Guid categoryId);
}