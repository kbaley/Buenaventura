namespace Buenaventura.Shared;

public class CategoryModel
{
    public Guid? CategoryId { get; set; }

    public string Name { get; set; } = "";

    public CategoryType Type { get; set; } = CategoryType.REGULAR;
    public bool IsFreeForm { get; set; } = false;
    public string CategoryClass { get; set; } = "";

    public Guid? InvoiceId { get; set; }
    public Guid? TransferAccountId { get; set; }
    public int TimesUsed { get; set; }
    public bool IncludeInReports { get; set; }
    public bool ExcludeFromTransactionReports { get; set; }
}