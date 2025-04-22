namespace Buenaventura.Shared;

public class IncomeExpenseDataPoint
{
    public DateOnly Date { get; set; }
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal CashFlow => Income - Expenses;
    public string Month => Date.ToString("MMM yyyy");
}