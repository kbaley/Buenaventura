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
            TransactionDate = transaction.TransactionDate,
            TransactionType = transaction.TransactionType,
            CategoryId = transaction.CategoryId,
            Vendor = transaction.Vendor,
            Description = transaction.Description,
            Amount = transaction.Amount,
            IsReconciled = transaction.IsReconciled,
            InvoiceId = transaction.InvoiceId,
            AmountInBaseCurrency = transaction.AmountInBaseCurrency,
            DownloadId = transaction.DownloadId
        };
    }
}