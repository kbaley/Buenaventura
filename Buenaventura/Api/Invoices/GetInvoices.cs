using Buenaventura.Client.Services;
using Buenaventura.Shared;
using FastEndpoints;
using IInvoiceService = Buenaventura.Services.IInvoiceService;

namespace Buenaventura.Api.Invoices;

public class GetInvoices(IInvoiceService invoiceService) : EndpointWithoutRequest<IEnumerable<InvoiceModel>>
{
    public override void Configure()
    {
        Get("/api/invoices");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var invoices = await invoiceService.GetInvoices();
        await SendAsync(invoices, cancellation: ct);
    }
}