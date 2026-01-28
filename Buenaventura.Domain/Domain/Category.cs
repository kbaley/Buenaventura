using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("categories")]
public class Category
{
    [Key] public Guid CategoryId { get; set; }

    [Required] public string Name { get; set; } = "";

    [Required] public string Type { get; set; } = "";
    /// <summary>
    /// Whether this is included in summary reports where we want to show only a few categories for major expenses
    /// </summary>
    public bool IncludeInReports { get; set; }
    public bool ExcludeFromTransactionReport { get; set; }

    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = [];
}