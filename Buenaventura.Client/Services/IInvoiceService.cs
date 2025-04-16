using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IInvoiceService : IAppService
{
    Task<IEnumerable<InvoiceModel>> GetInvoices();
    Task DeleteInvoice(Guid invoiceId);
    
    Task<int> GetNextInvoiceNumber();
    Task CreateInvoice(InvoiceModel invoice);
    Task<string> GetInvoiceTemplate();
    Task SaveInvoiceTemplate(string template);
}