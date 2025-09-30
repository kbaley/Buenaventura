using System.Text;
using FastEndpoints;
using Buenaventura.Services;

namespace Buenaventura.Api.Invoices;

public class ViewInvoice(IInvoiceGenerator invoiceGenerator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/invoices/{invoiceId}/view");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var invoiceId = Route<Guid>("invoiceId");
        var html = await invoiceGenerator.GenerateHtml(invoiceId);
        var bytes = Encoding.UTF8.GetBytes(html);
        await SendBytesAsync(bytes: bytes, contentType: "text/html", cancellation: ct);
    }
}
