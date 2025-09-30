using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientInvoiceService(HttpClient httpClient) : ClientService<InvoiceModel>("invoices", httpClient), IInvoiceService
{
    
    public async Task<IEnumerable<InvoiceModel>> GetInvoices()
    {
        throw new NotImplementedException();
        // return await GetAll();
    }

    public async Task DeleteInvoice(Guid invoiceId)
    {
        throw new NotImplementedException();
        // await Delete(invoiceId);
    }

    public async Task<int> GetNextInvoiceNumber()
    {
        throw new NotImplementedException();
        // return await GetItem<int>("nextinvoicenumber");
    }

    public async Task CreateInvoice(InvoiceModel invoice)
    {
        throw new NotImplementedException();
        // await Post(invoice);
    }

    public async Task<string> GetInvoiceTemplate()
    {
        throw new NotImplementedException();
        // var url = $"api/{Endpoint}/invoicetemplate";
        // var result = await Client.GetStringAsync(url);
        // return result;
    }

    public async Task SaveInvoiceTemplate(string template)
    {
        throw new NotImplementedException();
        // var url = $"api/{Endpoint}/invoicetemplate";
        // // Special handling because the template will be HTML
        // var content = JsonContent.Create(new { Template = template });
        // var response = await Client.PostAsync(url, content);
        // response.EnsureSuccessStatusCode();
    }

    public async Task EmailInvoice(Guid invoiceId)
    {
        throw new NotImplementedException();
        // var url = $"api/{Endpoint}/{invoiceId}/email";
        // var response = await Client.PostAsync(url, null);
        // response.EnsureSuccessStatusCode();
    }
}