using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("invoices")]
public class Invoice
{
    [Key] public Guid InvoiceId { get; set; }

    [Required] public string InvoiceNumber { get; set; }

    [Required] public DateTime Date { get; set; }

    [Required] public Customer Customer { get; set; }
    public Guid CustomerId { get; set; }

    public virtual ICollection<InvoiceLineItem> LineItems { get; set; }
    public decimal Balance { get; set; }

    public bool IsPaidInFull { get; set; }

    public DateTimeOffset? LastSentToCustomer { get; set; }
}