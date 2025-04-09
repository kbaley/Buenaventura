namespace Buenaventura.Shared;

public class CategoryDto
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = "";

    public string Type { get; set; } = "";
    public TransactionType TransactionType { get; set; } = TransactionType.REGULAR;

    public Guid? ParentCategoryId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? TransferAccountId { get; set; }
}