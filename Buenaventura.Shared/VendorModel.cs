namespace Buenaventura.Shared;

public class VendorModel
{
    public Guid VendorId { get; set; }

    public string Name { get; set; } = "";
    public Guid LastTransactionCategoryId { get; set; }
}