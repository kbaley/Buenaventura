using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IAccountsApi
{
    [Get("/api/accounts")]
    Task<IEnumerable<AccountWithBalance>> GetAccounts();
    
    [Get("/api/accounts/{accountId}")]
    Task<AccountWithBalance> GetAccount(Guid accountId);
    [Get("/api/accounts/{accountId}/transactions?search={search}&page={page}&pageSize={pageSize}")]
    Task<TransactionListModel> GetTransactions(Guid accountId, string search = "", int page = 0, int pageSize = 50);
    [Put("/api/transactions")]
    Task UpdateTransaction(TransactionForDisplay transaction);
}