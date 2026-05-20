using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("reimbursement_match_transactions")]
public class ReimbursementMatchTransaction
{
    public Guid ReimbursementMatchId { get; set; }
    public ReimbursementMatch? Match { get; set; }
    public Guid TransactionId { get; set; }
    public Transaction? Transaction { get; set; }
}
