namespace Buenaventura.Shared;

public class VendorDto
{
    public Guid VendorId { get; set; }

    public string Name { get; set; } = "";
    public Guid LastTransactionCategoryId { get; set; }
}