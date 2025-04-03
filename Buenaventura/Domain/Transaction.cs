using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Buenaventura.Shared;

namespace Buenaventura.Domain;

[Table("transactions")]
public class Transaction
{
    [Key] public Guid TransactionId { get; set; }

    public Guid AccountId { get; set; }
    [Required] public Account Account { get; set; } = new();
    public string? Vendor { get; set; }
    public string Description { get; set; } = "";

    public decimal Amount { get; set; }
    public bool IsReconciled { get; set; }
    public DateTime TransactionDate { get; set; }
    public Category Category { get; set; } = new();
    public Guid? CategoryId { get; set; }
    public DateTime EnteredDate { get; set; } = DateTime.Now;
    public Guid? InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = new();
    public TransactionType TransactionType { get; set; }
    public Transfer LeftTransfer { get; set; } = new();
    public Transfer RightTransfer { get; set; } = new();
    public decimal AmountInBaseCurrency { get; set; }
    public string? DownloadId { get; set; }
    public Guid? DividendInvestmentId { get; set; }

    public string GetCategoryDisplay()
    {
        return TransactionType switch
        {
            TransactionType.REGULAR => Category == null ? "" : Category.Name,
            TransactionType.INVOICE_PAYMENT => Invoice == null ? "PAYMENT" : "PAYMENT: " + Invoice.InvoiceNumber,
            TransactionType.TRANSFER => LeftTransfer == null
                ? "TRANSFER"
                : "TRANSFER: " + LeftTransfer.RightTransaction.Account.Name,
            TransactionType.INVESTMENT => "INVESTMENT",
            TransactionType.DIVIDEND => Category == null ? "DIVIDEND" : Category.Name,
            _ => "",
        };
    }

    public void SetAmountInBaseCurrency(string currency, decimal exchangeRate)
    {
        if (currency == "USD")
        {
            AmountInBaseCurrency = Amount;
        }
        else
        {
            AmountInBaseCurrency = Math.Round(Amount / exchangeRate, 2);
        }
    }
}
