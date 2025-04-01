using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("vendors")]
public class Vendor
{
    [Key] public Guid VendorId { get; set; }

    [Required] public string Name { get; set; }
    [Required] public Guid LastTransactionCategoryId { get; set; }
}