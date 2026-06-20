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
    
    [Get("/api/transactions/all?start={start}&end={end}")]
    Task<TransactionListModel> GetAllTransactions(DateTime start, DateTime end);
    
    [Get("/api/categories/{categoryId}/transactions?page={page}&pageSize={pageSize}")]
    Task<TransactionListModel> GetTransactionsByCategory(Guid categoryId, int page = 0, int pageSize = 50);

    [Get("/api/transactions/tags?includeTags={includeTags}&excludeTags={excludeTags}&page={page}&pageSize={pageSize}")]
    Task<TransactionListModel> GetTransactionsByTags(string includeTags = "", string excludeTags = "", int page = 0, int pageSize = 50);

    [Get("/api/transactions/available-tags")]
    Task<List<string>> GetAvailableTags();
    
    [Post("/api/accounts/{accountId}/transactions/bulk")]
    Task AddBulkTransactions(Guid accountId, CreateBulkTransactionsRequest request);
}
