using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("reimbursement_matches")]
public class ReimbursementMatch
{
    [Key] public Guid ReimbursementMatchId { get; set; }
    public Guid ReimbursementSettlementId { get; set; }
    public ReimbursementSettlement? Settlement { get; set; }
    public string Notes { get; set; } = "";
    public string AcceptedDifferenceReason { get; set; } = "";
    public ICollection<ReimbursementMatchTransaction> MatchTransactions { get; set; } = [];
}
