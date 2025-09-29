using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IAccountsApi
{
    [Get("/api/accounts")]
    Task<IEnumerable<AccountWithBalance>> GetAccounts();
    
    [Get("/api/accounts/{accountId}")]
    Task<AccountWithBalance> GetAccount(Guid accountId);
    
    [Post("/api/accounts/order")]
    Task SaveAccountOrder(List<OrderedAccount> accountOrders);
    
    [Put("/api/accounts")]
    Task UpdateAccount(AccountWithBalance account);
}