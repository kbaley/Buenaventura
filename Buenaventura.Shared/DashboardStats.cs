namespace Buenaventura.Shared;

public class DashboardStats
{
    public decimal CreditCardBalance { get; set; }
    public decimal LiquidAssetBalance { get; set; }
    public decimal ExpensesThisMonth { get; set; }
    public IEnumerable<ReportDataPoint> NetWorth { get; set; } = [];
    public IEnumerable<ReportDataPoint> Investments { get; set; } = [];
    public IEnumerable<ReportDataPoint> Expenses { get; set; } = [];
    public IEnumerable<IncomeExpenseDataPoint> IncomeExpenses { get; set; } = [];
}