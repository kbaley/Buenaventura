namespace Buenaventura.Shared;

public class InvoiceDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = "";
    public Guid CustomerId { get; set; }
    public decimal Balance { get; set; }
    public bool IsPaidInFull { get; set; }
    public decimal Total { get; set; }
}