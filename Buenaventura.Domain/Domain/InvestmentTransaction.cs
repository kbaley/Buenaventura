using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("investment_transactions")]
public class InvestmentTransaction
{
    [Key] public Guid InvestmentTransactionId { get; set; }
    [Required] public Guid InvestmentId { get; set; }
    public decimal Shares { get; set; }
    public decimal Price { get; set; }
    public DateTime Date { get; set; }

    public Guid TransactionId { get; set; }
    public Transaction Transaction { get; set; }
}