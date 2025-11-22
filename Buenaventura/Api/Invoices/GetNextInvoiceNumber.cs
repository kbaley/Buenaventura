using FastEndpoints;
using IInvoiceService = Buenaventura.Services.IInvoiceService;

namespace Buenaventura.Api.Invoices;

public class GetNextInvoiceNumber(IInvoiceService invoiceService) : EndpointWithoutRequest<int>
{

    public override void Configure()
    {
        Get("/api/invoices/nextinvoicenumber");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var nextInvoiceNumber = await invoiceService.GetNextInvoiceNumber();
        await SendAsync(nextInvoiceNumber, cancellation: ct);
    }
}