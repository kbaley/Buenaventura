using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IInvoiceService : IAppService
{
    public Task<IEnumerable<InvoiceModel>> GetInvoices();
    public Task DeleteInvoice(Guid invoiceId);
}