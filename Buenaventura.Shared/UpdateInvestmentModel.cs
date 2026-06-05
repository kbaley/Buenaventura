namespace Buenaventura.Shared;

public class UpdateInvestmentModel
{
    public Guid InvestmentId { get; set; }
    public string Name { get; set; } = "";
    public string Symbol { get; set; } = "";
    public bool DontRetrievePrices { get; set; }
    public string Currency { get; set; } = "";
    public Guid? CategoryId { get; set; }
    public bool PaysDividends { get; set; }
}
