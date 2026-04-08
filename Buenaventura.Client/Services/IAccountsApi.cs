using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IAccountsApi
{
    [Get("/api/accounts")]
    Task<IEnumerable<AccountWithBalance>> GetAccounts(bool includeHidden = false);

    [Post("/api/accounts")]
    Task<AccountWithTransactions> CreateAccount(AccountForPosting account);
    
    [Get("/api/accounts/{accountId}")]
    Task<AccountWithBalance> GetAccount(Guid accountId);
    
    [Post("/api/accounts/order")]
    Task SaveAccountOrder(List<OrderedAccount> accountOrders);
    
    [Put("/api/accounts")]
    Task UpdateAccount(AccountWithBalance account);

    [Delete("/api/accounts/{accountId}")]
    Task<DeleteAccountResponse> DeleteAccount(Guid accountId);
}
