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
    [Get("/api/expenses?includeTags={includeTags}&excludeTags={excludeTags}")]
    Task<IEnumerable<ReportDataPoint>> GetExpenseCategoryBreakdown(string includeTags = "", string excludeTags = "");
    
    [Get("/api/expenses/averages?includeTags={includeTags}&excludeTags={excludeTags}")]
    Task<IEnumerable<ExpenseAveragesDataPoint>> GetExpenseAveragesData(string includeTags = "", string excludeTags = "");
    
    /// <summary>
    /// Get a breakdown of expense totals by category and month for the last 12 months
    /// </summary>
    [Get("/api/expenses/totalsbymonth?includeTags={includeTags}&excludeTags={excludeTags}")]
    Task<CategoryTotals> GetExpenseTotalsByMonth(string includeTags = "", string excludeTags = "");
    
    [Get("/api/expenses/thismonth?includeTags={includeTags}&excludeTags={excludeTags}")]
    Task<decimal> GetThisMonthExpenses(string includeTags = "", string excludeTags = "");

    [Get("/api/expenses/category/{categoryId}?includeTags={includeTags}&excludeTags={excludeTags}")]
    Task<ExpenseCategoryPageData> GetExpenseCategoryPageData(Guid categoryId, string includeTags = "", string excludeTags = "");

    [Get("/api/expenses/vendors?includeTags={includeTags}&excludeTags={excludeTags}")]
    Task<List<ReportDataPoint>> GetVendorData(string includeTags = "", string excludeTags = "");
}
