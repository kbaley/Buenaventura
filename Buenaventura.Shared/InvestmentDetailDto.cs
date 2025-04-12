namespace Buenaventura.Shared;

public class InvestmentDetailDto
{
    public Guid InvestmentId { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public string Currency { get; set; }
    public bool DontRetrievePrices { get; set; }
    public decimal Shares { get; set; }
    public decimal LastPrice { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal BookValue { get; set; }
    public double TotalAnnualizedReturn { get; set; }
    public decimal TotalReturn { get; set; } 
    public string CategoryName { get; set; }
    public Guid CategoryId { get; set; }
    public decimal CategoryPercentage { get; set; }
    public IEnumerable<InvestmentTransactionDto> Transactions { get; set; }
    public IEnumerable<InvestmentDividendDto> Dividends { get; set; }
}