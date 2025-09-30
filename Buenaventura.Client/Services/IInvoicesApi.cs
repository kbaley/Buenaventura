using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IInvoicesApi
{
    [Get("/api/invoices")]
    Task<IEnumerable<InvoiceModel>> GetInvoices();
    [Delete("/api/invoices/{invoiceId}")]
    Task DeleteInvoice(Guid invoiceId);
    
    [Get("/api/invoices/nextinvoicenumber")]
    Task<int> GetNextInvoiceNumber();
    
    [Post("/api/invoices")]
    Task CreateInvoice(InvoiceModel invoice);
    
    [Get("/api/invoices/invoicetemplate")]
    Task<string> GetInvoiceTemplate();
    
    [Post("/api/invoices/invoicetemplate")]
    Task SaveInvoiceTemplate(InvoiceTemplateModel invoiceTemplate);
    
    [Post("/api/invoices/{invoiceId}/email")]
    Task EmailInvoice(Guid invoiceId);
}