using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientInvoiceService(HttpClient httpClient) : IInvoiceService
{
    public async Task<IEnumerable<InvoiceModel>> GetInvoices()
    {
        var url = "api/invoices/?type=transactioncategory";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<InvoiceModel>>(url);
        return result ?? Array.Empty<InvoiceModel>();
    }

    public async Task DeleteInvoice(Guid invoiceId)
    {
        await httpClient.DeleteAsync($"api/invoices/{invoiceId}");
    }
}