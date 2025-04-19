namespace Buenaventura.Shared;

public class BuySellModel
{
    public Guid InvestmentId { get; set; }
    public decimal Shares { get; set; }
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public Guid AccountId { get; set; }
}