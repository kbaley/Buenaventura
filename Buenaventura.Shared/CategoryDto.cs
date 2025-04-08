namespace Buenaventura.Shared;

public class CategoryDto
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = "";

    public string Type { get; set; } = "";

    public Guid? ParentCategoryId { get; set; }
    public Guid? InvoiceId { get; set; }
}

public class InvoiceAsCategory
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = "";
    public Guid CustomerId { get; set; }
    public decimal Balance { get; set; }
    public bool IsPaidInFull { get; set; }
}
