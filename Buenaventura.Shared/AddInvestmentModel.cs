namespace Buenaventura.Shared;

public class AddInvestmentModel
{
    public string Name { get; set; } = "";
    public string Symbol { get; set; } = "";
    public string Currency { get; set; } = "";
    public bool DontRetrievePrices { get; set; }
    public Guid? CategoryId { get; set; }
    public bool PaysDividends { get; set; }
    public decimal Price { get; set; }
    public DateTime? Date { get; set; }
    public decimal Shares { get; set; }
    public Guid? AccountId { get; set; }
}