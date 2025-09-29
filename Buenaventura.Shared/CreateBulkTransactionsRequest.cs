namespace Buenaventura.Shared;

public record CreateBulkTransactionsRequest(Guid AccountId, List<TransactionForDisplay> Transactions);