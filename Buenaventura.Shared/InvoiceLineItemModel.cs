namespace Buenaventura.Shared;

public class InvoiceLineItemModel
{
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public string Description { get; set; } = "";
    public CategoryModel? Category { get; set; }
    
}