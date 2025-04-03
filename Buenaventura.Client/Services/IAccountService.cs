using System.Net.Http.Json;
using Buenaventura.Dtos;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IAccountService : IAppService
{
     Task<IEnumerable<AccountWithBalance>> GetAccounts();
     Task<TransactionListModel> GetTransactions(Guid accountId, bool loadAll = false);
}

public class ClientAccountService(HttpClient httpClient) : IAccountService
{
    public async Task<IEnumerable<AccountWithBalance>> GetAccounts()
    {
        var url = "api/accounts";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<AccountWithBalance>>(url);
        return result ?? [];
    }

    public async Task<TransactionListModel> GetTransactions(Guid accountId, bool loadAll = false)
    {
        var url = $"api/transactions?accountId={accountId}";
        var result = await httpClient.GetFromJsonAsync<TransactionListModel>(url);
        return result ?? new TransactionListModel();
    }
}