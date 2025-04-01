namespace Buenaventura.Dtos;

public class InvestmentForUpdateDto {
    public Guid InvestmentId { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public bool DontRetrievePrices { get; set; }
    public string Currency { get; set; }
    public Guid CategoryId { get; set; }
    public decimal CategoryPercentage { get; set; }
    public bool PaysDividends { get; set; }
}