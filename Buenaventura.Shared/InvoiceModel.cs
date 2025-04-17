namespace Buenaventura.Shared;

public class InvoiceModel
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public Guid CustomerId { get; set; }
    public decimal Balance { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset? LastSentToCustomer { get; set; }
    public IEnumerable<InvoiceLineItemModel> LineItems { get; set; } = new List<InvoiceLineItemModel>();
}