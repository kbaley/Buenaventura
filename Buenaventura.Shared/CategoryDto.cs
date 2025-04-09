namespace Buenaventura.Shared;

public class CategoryDto
{
    public Guid? CategoryId { get; set; }

    public string Name { get; set; } = "";

    public CategoryType Type { get; set; } = CategoryType.REGULAR;
    public bool IsFreeForm { get; set; } = false;

    public Guid? InvoiceId { get; set; }
    public Guid? TransferAccountId { get; set; }
}