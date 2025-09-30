using FastEndpoints;
using Buenaventura.Client.Services;
using IInvoiceService = Buenaventura.Services.IInvoiceService;

namespace Buenaventura.Api.Invoices;

public class EmailInvoice(IInvoiceService invoiceService) : EndpointWithoutRequest<object>
{
    public override void Configure()
    {
        Post("/api/invoices/{invoiceId}/email");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var invoiceId = Route<Guid>("invoiceId");
        await invoiceService.EmailInvoice(invoiceId);
        await SendOkAsync(new { Status = "Email sent successfully" }, ct);
    }
}
