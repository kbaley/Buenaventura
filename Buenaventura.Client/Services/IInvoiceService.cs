using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IInvoiceService : IAppService
{
    public Task<IEnumerable<InvoiceDto>> GetInvoices();
    public Task DeleteInvoice(Guid invoiceId);
}