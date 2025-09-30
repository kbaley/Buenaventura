using FastEndpoints;
using Buenaventura.Data;
using Buenaventura.Domain;

namespace Buenaventura.Api.Invoices;

public class DeleteInvoice(BuenaventuraDbContext context) : EndpointWithoutRequest<Invoice>
{
    public override void Configure()
    {
        Delete("/api/invoices/{id}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var invoice = await context.Invoices.FindAsync([id], ct);
        if (invoice is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        context.Invoices.Remove(invoice);
        await context.SaveChangesAsync(ct);
        await SendOkAsync(invoice, ct);
    }
}
