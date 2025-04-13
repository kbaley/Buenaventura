using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientInvoiceService(HttpClient httpClient) : IInvoiceService
{
    public async Task<IEnumerable<InvoiceDto>> GetInvoices()
    {
        var url = "api/invoices/?type=transactioncategory";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<InvoiceDto>>(url);
        return result ?? Array.Empty<InvoiceDto>();
    }

    public async Task DeleteInvoice(Guid invoiceId)
    {
        await httpClient.DeleteAsync($"api/invoices/{invoiceId}");
    }
}