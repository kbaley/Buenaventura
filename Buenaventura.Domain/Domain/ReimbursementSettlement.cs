using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("reimbursement_settlements")]
public class ReimbursementSettlement
{
    [Key] public Guid ReimbursementSettlementId { get; set; }
    public string Name { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedDate { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<ReimbursementMatch> Matches { get; set; } = [];
}
