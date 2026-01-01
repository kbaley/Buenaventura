namespace Buenaventura.Shared;

public class ExpenseCategoryPageData
{
    public CategoryModel Category { get; set; } = new();
    public decimal ThisMonthSpending { get; set; }
    public decimal LastMonthSpending { get; set; }
    public List<ReportDataPoint> LastTwelveMonthsData { get; set; } = [];
    public List<ReportDataPoint> PreviousTwelveMonthsData { get; set; } = [];
    public List<ReportDataPoint> ComparisonData { get; set; } = [];
    public List<ReportDataPoint> VendorData { get; set; } = [];
}
