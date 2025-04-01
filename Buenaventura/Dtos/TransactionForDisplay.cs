using Buenaventura.Domain;

namespace Buenaventura.Dtos;

public class TransactionForDisplay
{
    public Guid TransactionId { get; set; }
    public string Vendor { get; set; }
    public string Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CategoryName { get; set; }
    public Guid? CategoryId { get; set; }
    public string CategoryDisplay { get; set; }
    public string AccountName { get; set; }
    public Guid? AccountId { get; set; }
    public decimal? Debit { get; set; }
    public decimal? Credit { get; set; }
    public decimal Amount { get; set; }
    public Guid? RelatedTransactionId { get; set; }
    public string RelatedAccountName { get; set; }
    public Guid? RelatedAccountId { get; set; }
    public DateTime EnteredDate { get; set; }
    public bool IsReconciled { get; set; }
    public Guid? InvoiceId { get; set; }
    public string InvoiceNumber { get; set; }
    public TRANSACTION_TYPE TransactionType { get; set; }
    public decimal AmountInBaseCurrency { get; set; }
    public decimal RunningTotal { get; set; }
    public string DownloadId { get; set; }
    public void SetAmount()
    {
        Amount = Debit.HasValue ? (0 - Debit.Value) : Credit.Value;
    }

    public void SetDebitAndCredit()
    {
        if (Amount < 0)
        {
            Debit = 0 - Amount;
        }
        else
        {
            Credit = Amount;
        }
    }

    // Creates a shallow copy of a transaction with none of the objects mapped
    public Transaction ShallowMap()
    {
        return new Transaction
        {
            TransactionId = TransactionId,
            AccountId = AccountId.Value,
            TransactionDate = TransactionDate,
            TransactionType = TransactionType,
            CategoryId = CategoryId,
            Vendor = Vendor,
            Description = Description,
            Amount = Amount,
            IsReconciled = IsReconciled,
            InvoiceId = InvoiceId,
            AmountInBaseCurrency = AmountInBaseCurrency,
            DownloadId = DownloadId
        };
    }
}