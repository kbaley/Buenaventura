namespace Buenaventura.Shared;

public record BulkTagTransactionsRequest(Guid AccountId, List<Guid> TransactionIds, string Tag);
