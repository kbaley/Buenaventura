using Buenaventura.Client.Services;
using Buenaventura.Shared;
using FastEndpoints;
using IInvoiceService = Buenaventura.Services.IInvoiceService;

namespace Buenaventura.Api.Invoices;

public class SaveInvoiceTemplate(IInvoiceService invoiceService) : Endpoint<InvoiceTemplateModel>
{
    public override void Configure()
    {
        Post("/api/invoices/invoicetemplate");
    }

    public override async Task HandleAsync(InvoiceTemplateModel req, CancellationToken ct)
    {
        await invoiceService.SaveInvoiceTemplate(req.Template);
        await SendOkAsync(ct);
    }
}
