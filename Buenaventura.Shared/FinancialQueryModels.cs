namespace Buenaventura.Shared;

public sealed record FinancialTransactionResult(
    Guid TransactionId,
    DateTime Date,
    string Type,
    string Category,
    string Account,
    string? Vendor,
    string? Description,
    decimal AmountInBaseCurrency);

public sealed record FinancialSummaryResult(
    string Type,
    string Category,
    decimal AmountInBaseCurrency,
    int EntryCount);

public sealed record FinancialCategoryResult(Guid CategoryId, string Name, string Type);
