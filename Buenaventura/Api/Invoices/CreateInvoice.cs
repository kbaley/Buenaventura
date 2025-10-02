using Buenaventura.Shared;
using FastEndpoints;
using IInvoiceService = Buenaventura.Services.IInvoiceService;

namespace Buenaventura.Api.Invoices;

public class CreateInvoice(IInvoiceService invoiceService) : Endpoint<InvoiceModel>
{
    public override void Configure()
    {
        Post("/api/invoices");
    }

    public override async Task HandleAsync(InvoiceModel req, CancellationToken ct)
    {
        await invoiceService.CreateInvoice(req);
        await SendOkAsync(ct);
    }
}
