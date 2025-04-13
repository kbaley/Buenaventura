using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

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

    public async Task UpdateTransaction(TransactionForDisplay transaction)
    {
        var url = $"api/transactions/{transaction.TransactionId}";
        var result = await httpClient.PutAsJsonAsync(url, transaction);
        if (result.IsSuccessStatusCode)
        {
            return;
        }
        throw new Exception(result.ReasonPhrase);
    }

    public async Task AddTransaction(Guid accountId, TransactionForDisplay transaction)
    {
        var url = $"api/accounts/{accountId}/transactions";
        var result = await httpClient.PostAsJsonAsync(url, transaction);
        if (result.IsSuccessStatusCode)
        {
            return;
        }
        throw new Exception(result.ReasonPhrase);
    }

    public async Task DeleteTransaction(Guid transactionId)
    {
        var url = $"api/transactions/{transactionId}";
        var result = await httpClient.DeleteAsync(url);
        if (result.IsSuccessStatusCode)
        {
            return;
        }
        throw new Exception(result.ReasonPhrase);
    }
}