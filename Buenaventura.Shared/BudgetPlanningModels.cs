namespace Buenaventura.Shared;

public class BudgetPlanningSummary
{
    public DateTime GeneratedAt { get; set; }
    public DateTime HistoryStart { get; set; }
    public DateTime HistoryEnd { get; set; }
    public int MonthsAnalyzed { get; set; }
    public decimal TotalAverageMonthlySpend { get; set; }
    public decimal TotalSuggestedMonthlyBudget { get; set; }
    public decimal RecurringMonthlyTotal { get; set; }
    public List<RecurringExpenseModel> RecurringExpenses { get; set; } = [];
    public List<BudgetCategoryRecommendation> BudgetCategories { get; set; } = [];
}

public class RecurringExpenseModel
{
    public string Vendor { get; set; } = "";
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public decimal AverageAmount { get; set; }
    public decimal LastAmount { get; set; }
    public DateTime LastTransactionDate { get; set; }
    public int OccurrenceCount { get; set; }
    public int MonthCount { get; set; }
    public string Cadence { get; set; } = "";
    public decimal Confidence { get; set; }
    public string Explanation { get; set; } = "";
}

public class BudgetCategoryRecommendation
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public decimal AverageMonthlySpend { get; set; }
    public decimal MedianMonthlySpend { get; set; }
    public decimal SuggestedMonthlyBudget { get; set; }
    public decimal RecurringMonthlyAmount { get; set; }
    public decimal CurrentMonthSpend { get; set; }
    public int MonthsWithSpend { get; set; }
    public bool IsCurrentMonthHigherThanBudget { get; set; }
    public string Insight { get; set; } = "";
}
