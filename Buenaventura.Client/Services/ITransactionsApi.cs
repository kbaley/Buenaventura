using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface ITransactionsApi
{
    [Get("/api/accounts/{accountId}/transactions?search={search}&page={page}&pageSize={pageSize}")]
    Task<TransactionListModel> GetTransactions(Guid accountId, string search = "", int page = 0, int pageSize = 50);
    
    [Put("/api/transactions")]
    Task UpdateTransaction(TransactionForDisplay transaction);
    
    [Post("/api/accounts/{accountId}/transactions")]
    Task AddTransaction(Guid accountId, TransactionForDisplay transaction);
    
    [Delete("/api/transactions/{transactionId}")]
    Task DeleteTransaction(Guid transactionId);
    
    [Get("/api/accounts/{accountId}/transactions/duplicates")]
    Task<TransactionListModel> GetPotentialDuplicateTransactions(Guid accountId);
    
    [Get("/api/accounts/{accountId}/transactions/all?start={start}&end={end}")]
    Task<TransactionListModel> GetAllTransactions(Guid accountId, DateTime start, DateTime end);
    
    [Get("/api/expenses/category/{categoryId}/transactions?page={page}&pageSize={pageSize}")]
    Task<TransactionListModel> GetTransactionsByCategory(Guid categoryId, int page = 0, int pageSize = 50);
    
    [Post("/api/accounts/{accountId}/transactions/bulk")]
    Task AddBulkTransactions(Guid accountId, CreateBulkTransactionsRequest request);
}