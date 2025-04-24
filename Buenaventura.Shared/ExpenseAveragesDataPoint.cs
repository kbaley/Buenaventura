namespace Buenaventura.Shared;

public class ExpenseAveragesDataPoint
{
    public string Category { get; set; } = string.Empty;
    public decimal Last30Days { get; set; }
    public decimal Last90DaysAverage { get; set; }
    public decimal Last360DaysAverage { get; set; }
}