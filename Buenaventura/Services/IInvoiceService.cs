using Buenaventura.Shared;

namespace Buenaventura.Services;

public interface IInvoiceService : IServerAppService
{
    Task<IEnumerable<InvoiceModel>> GetInvoices();
    Task<int> GetNextInvoiceNumber();
    Task CreateInvoice(InvoiceModel invoice);
    Task<string> GetInvoiceTemplate();
    Task SaveInvoiceTemplate(string template);
    Task EmailInvoice(Guid invoiceId);
}