namespace Buenaventura.Shared;

public class InvestmentTransactionDto
{
    public Guid InvestmentTransactionId { get; set; }
    public decimal Shares { get; set; }
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public string SourceAccountName { get; set; } = "";
}