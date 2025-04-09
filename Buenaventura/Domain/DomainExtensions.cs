using Buenaventura.Shared;

namespace Buenaventura.Domain;

public static class DomainExtensions
{
    
    public static Transaction ShallowMap(this TransactionForDisplay transaction)
    {
        return new Transaction
        {
            TransactionId = transaction.TransactionId,
            AccountId = transaction.AccountId!.Value,
            TransactionDate = DateTime.SpecifyKind(transaction.TransactionDate, DateTimeKind.Utc),
            TransactionType = transaction.TransactionType,
            CategoryId = transaction.Category.CategoryId,
            Vendor = transaction.Vendor,
            Description = transaction.Description,
            Amount = transaction.Amount,
            IsReconciled = transaction.IsReconciled,
            InvoiceId = transaction.Category.InvoiceId,
            AmountInBaseCurrency = transaction.AmountInBaseCurrency,
            DownloadId = transaction.DownloadId
        };
    }
}