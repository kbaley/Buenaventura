using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("accounts")]
public class Account
{
    [Key] public Guid AccountId { get; set; }

    public string Name { get; set; }

    [NotMapped] public decimal CurrentBalance { get; set; }

    [DefaultValue(false)] public bool IsHidden { get; set; }

    [Required] [DefaultValue("USD")] public string Currency { get; set; }

    public string Vendor { get; set; }

    public string AccountType { get; set; }

    public decimal? MortgagePayment { get; set; }

    public string MortgageType { get; set; }
    public int DisplayOrder { get; set; }

    public virtual IEnumerable<Transaction> Transactions { get; set; }
}