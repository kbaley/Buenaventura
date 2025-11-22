namespace Buenaventura.Dtos;

public class InvoiceLineItemsForPosting
{
    public Guid InvoiceLineItemId { get; set; }
    public decimal Quantity { get; set; } = 0;
    public decimal UnitAmount { get; set; } = 0;
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Unchanged";
    public Guid InvoiceId { get; set; }
    public Guid CategoryId { get; set; }
}