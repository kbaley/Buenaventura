namespace Buenaventura.Shared;

public class ReportDataPoint
{
    public Guid? Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}