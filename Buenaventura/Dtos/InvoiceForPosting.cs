namespace Buenaventura.Dtos;

public class InvoiceForPosting
{
    public Guid InvoiceId { get; set; }
    public DateTime Date { get; set; }
    public Guid CustomerId { get; set; }
    public List<InvoiceLineItemsForPosting> LineItems { get; set; } = [];
    public string CustomerName { get; set; } = "";
    public string CustomerStreetAddress { get; set; } = "";
    public string CustomerCity { get; set; } = "";
    public string CustomerRegion { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string InvoiceNumber { get; set; } = "";
    public DateTime? LastSentToCustomer { get; set; }

    public decimal Balance { get; set; }

    public decimal GetLineItemTotal() {
        return LineItems.Where(li => li.Status != "Deleted").Sum(li => li.Quantity * li.UnitAmount);
    }
}