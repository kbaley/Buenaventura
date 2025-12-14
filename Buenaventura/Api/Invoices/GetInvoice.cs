using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api.Invoices;

public class GetInvoice(IInvoiceService invoiceService) : EndpointWithoutRequest<InvoiceModel>
{
    public override void Configure()
    {
        Get("/api/invoices/{id:guid}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var invoice = await invoiceService.GetInvoice(id);
        await SendOkAsync(invoice, ct);
    }
}