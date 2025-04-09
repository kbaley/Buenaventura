using Buenaventura.Domain;
using Buenaventura.Shared;

namespace Buenaventura;

public static class Mappers
{
    public static TransactionForDisplay ToDto(this Transaction transaction)
    {
        return new TransactionForDisplay
        {
            AccountId = transaction.AccountId,
            AccountName = transaction.Account?.Name ?? "",
            Amount = transaction.Amount,
            AmountInBaseCurrency = transaction.AmountInBaseCurrency,
            Category = new CategoryDto
                {
                    CategoryId = transaction.Category?.CategoryId,
                    Name = transaction.GetCategoryDisplay(),
                },
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            EnteredDate = transaction.EnteredDate,
            Vendor = transaction.Vendor,
            TransactionId = transaction.TransactionId,
        };
    }
}