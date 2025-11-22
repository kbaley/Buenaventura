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
            TransactionType = GetTransactionType(transaction.Category.Type),
            CategoryId = transaction.Category.CategoryId == Guid.Empty ? null : transaction.Category.CategoryId,
            Vendor = transaction.Vendor,
            Description = transaction.Description,
            Amount = transaction.Amount,
            IsReconciled = transaction.IsReconciled,
            InvoiceId = transaction.Category.InvoiceId,
            AmountInBaseCurrency = transaction.AmountInBaseCurrency,
            DownloadId = transaction.DownloadId
        };
    }

    private static TransactionType GetTransactionType(CategoryType categoryType)
    {
        switch (categoryType)
        {
            case CategoryType.REGULAR:
            case CategoryType.FREEFORM:
                return TransactionType.REGULAR;
            case CategoryType.INVOICE_PAYMENT:
                return TransactionType.INVOICE_PAYMENT;
            case CategoryType.DIVIDEND:
                return TransactionType.DIVIDEND;
            case CategoryType.INVESTMENT:
                return TransactionType.INVESTMENT;
            case CategoryType.MORTGAGE_PAYMENT:
                return TransactionType.MORTGAGE_PAYMENT;
            case CategoryType.TRANSFER:
                return TransactionType.TRANSFER;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(categoryType), categoryType, null);
        }
    }
}