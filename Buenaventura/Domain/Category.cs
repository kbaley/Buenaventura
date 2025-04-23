using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("categories")]
public class Category
{
    [Key] public Guid CategoryId { get; set; }

    [Required] public string Name { get; set; } = "";

    [Required] public string Type { get; set; } = "";
    public bool IncludeInReports { get; set; }

    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
}