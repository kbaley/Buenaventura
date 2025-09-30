using FastEndpoints;
using Buenaventura.Data;
using Buenaventura.Services;

namespace Buenaventura.Api.Invoices;

public class DownloadInvoice(BuenaventuraDbContext context, IInvoiceGenerator invoiceGenerator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/invoices/{invoiceId}/download");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var invoiceId = Route<Guid>("invoiceId");
        var invoice = await context.FindInvoiceEager(invoiceId);
        var invoiceBytes = await invoiceGenerator.GeneratePdf(invoiceId);
        await SendBytesAsync(bytes: invoiceBytes, fileName: $"Invoice-{invoice.InvoiceNumber}.pdf", contentType: "application/pdf", cancellation: ct);
    }
}
