using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientInvoiceService(HttpClient httpClient) : ClientService<InvoiceModel>("invoices", httpClient), IInvoiceService
{
    
    public async Task<IEnumerable<InvoiceModel>> GetInvoices()
    {
        return await GetAll();
    }

    public async Task DeleteInvoice(Guid invoiceId)
    {
        await Delete(invoiceId);
    }

    public async Task<int> GetNextInvoiceNumber()
    {
        var url = "api/invoices/nextinvoicenumber";
        var result = await Client.GetFromJsonAsync<int>(url);
        return result;
    }

    public async Task CreateInvoice(InvoiceModel invoice)
    {
        await Create(invoice);
    }
}