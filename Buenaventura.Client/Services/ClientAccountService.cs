using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientAccountService(HttpClient httpClient) : ClientService<AccountWithBalance>("accounts", httpClient), IAccountService
{
    public async Task<IEnumerable<AccountWithBalance>> GetAccounts()
    {
        return await GetAll();
    }

    public async Task<TransactionListModel> GetTransactions(Guid accountId, string search = "", int page = 0, int pageSize = 50)
    {
        var url = $"{accountId}/transactions?search={search}&page={page}&pageSize={pageSize}";
        return await GetItem<TransactionListModel>(url);
    }

    public async Task<AccountWithBalance> GetAccount(Guid id)
    {
        return await Get(id);
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
        var url = $"{accountId}/transactions";
        await PostItem(url, transaction);
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

    public Task SaveAccountOrder(List<OrderedAccount> accountOrders)
    {
        var url = "api/accounts/order";
        return httpClient.PostAsJsonAsync(url, accountOrders);
    }

    public async Task<TransactionListModel> GetPotentialDuplicateTransactions(Guid accountId)
    {
        var url = $"{accountId}/transactions/duplicates";
        return await GetItem<TransactionListModel>(url);
    }

    public async Task UpdateAccount(AccountWithBalance account)
    {
        var url = $"api/accounts";
        var result = await Client.PutAsJsonAsync(url, account);
        if (result.IsSuccessStatusCode)
        {
            return;
        }
        throw new Exception(result.ReasonPhrase);
    }

    public async Task<TransactionListModel> GetAllTransactions(Guid accountId, DateTime start, DateTime end)
    {
        // Get all transactions without pagination for duplicate checking
        var url = $"{accountId}/transactions/all?start={start}&end={end}";
        return await GetItem<TransactionListModel>(url);
    }

    public async Task<bool> AddBulkTransactions(Guid accountId, List<TransactionForDisplay> transactions)
    {
        var url = $"api/accounts/{accountId}/transactions/bulk";
        var response = await httpClient.PostAsJsonAsync(url, new { Transactions = transactions });
        return response.IsSuccessStatusCode;
    }
}