using Buenaventura.Data;
using Buenaventura.Dtos;
using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api.Invoices;

public class PutInvoice(IInvoiceService invoiceService) : Endpoint<InvoiceModel, InvoiceModel>
{
    public override void Configure()
    {
        Put("/api/invoices");
    }

    public override async Task HandleAsync(InvoiceModel req, CancellationToken ct)
    {
        await invoiceService.UpdateInvoice(req);
        await SendOkAsync(req, ct);
    }
}
