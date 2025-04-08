using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IInvoiceService : IAppService
{
    public Task<IEnumerable<InvoiceAsCategory>> GetInvoicesForTransactionCategories();
}

public class ClientInvoiceService(HttpClient httpClient) : IInvoiceService
{
    public async Task<IEnumerable<InvoiceAsCategory>> GetInvoicesForTransactionCategories()
    {
        var url = "api/invoices/?type=transactioncategory";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<InvoiceAsCategory>>(url);
        return result ?? Array.Empty<InvoiceAsCategory>();
    }
}