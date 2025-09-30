using System.Text;
using Buenaventura.Client.Services;
using FastEndpoints;
using IInvoiceService = Buenaventura.Services.IInvoiceService;

namespace Buenaventura.Api.Invoices;

public class GetInvoiceTemplate(IInvoiceService invoiceService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/invoices/invoicetemplate");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var html = await invoiceService.GetInvoiceTemplate();
        var bytes = Encoding.UTF8.GetBytes(html);
        await SendBytesAsync(bytes: bytes, contentType: "text/html", cancellation: ct);
    }
}
