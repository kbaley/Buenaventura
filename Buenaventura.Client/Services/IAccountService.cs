using System.Net.Http.Json;
using Buenaventura.Dtos;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IAccountService : IAppService
{
     Task<IEnumerable<AccountWithBalance>> GetAccounts();
     Task<TransactionListModel> GetTransactions(Guid accountId, string search = "", int page = 0, int pageSize = 50);
     Task<AccountWithBalance> GetAccount(Guid id);
}

public class ClientAccountService(HttpClient httpClient) : IAccountService
{
    public async Task<IEnumerable<AccountWithBalance>> GetAccounts()
    {
        var url = "api/accounts";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<AccountWithBalance>>(url);
        return result ?? [];
    }

    public async Task<TransactionListModel> GetTransactions(Guid accountId, string search = "", int page = 0, int pageSize = 50)
    {
        var url = $"api/accounts/{accountId}/transactions?search={search}&page={page}&pageSize={pageSize}";
        var result = await httpClient.GetFromJsonAsync<TransactionListModel>(url);
        return result ?? new TransactionListModel();
    }

    public async Task<AccountWithBalance> GetAccount(Guid id)
    {
        var url = $"api/accounts/{id}";
        var result = await httpClient.GetFromJsonAsync<AccountWithBalance>(url);
        return result ?? new AccountWithBalance();
    }
}