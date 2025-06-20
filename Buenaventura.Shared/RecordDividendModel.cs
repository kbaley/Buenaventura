namespace Buenaventura.Shared;

public class RecordDividendModel
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public Guid AccountId { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal Total { get; set; }
}