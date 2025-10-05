namespace Buenaventura.MCP.Models;

public class Transaction
{
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Vendor { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
}