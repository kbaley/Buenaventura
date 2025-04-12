namespace Buenaventura.Shared;

public class InvestmentDividendDto
{
    public Guid InvestmentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public Guid AccountId { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal Total { get; set; }
}

public class InvestmentForListDto
{
    public Guid InvestmentId { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public decimal Shares { get; set; }
    public decimal LastPrice { get; set; }
    public DateTime LastPriceRetrievalDate { get; set; }
    public decimal AveragePrice { get; set; }
    public string Currency { get; set; }
    public bool DontRetrievePrices { get; set; }
    public decimal CurrentValue { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; }
    // Represents a price for buying and selling
    public decimal Price { get; set; }
    // The date of a buy or sell
    public DateTime Date { get; set; }
    public double AnnualizedIrr { get; set; }
    public decimal BookValue { get; set; }
    public string CategoryName { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal CategoryPercentage { get; set; }
    public bool PaysDividends { get; set; }
    public decimal TotalSharesBought { get; set; }
}