using Buenaventura.Services;
using FastEndpoints;

namespace Buenaventura.Api;

internal sealed record DeleteVendorRequest(Guid Id);

internal class DeleteVendor(IVendorService vendorService) : Endpoint<DeleteVendorRequest>
{
    public override void Configure()
    {
        Delete("/api/vendors/{Id}");
    }

    public override async Task HandleAsync(DeleteVendorRequest req, CancellationToken ct)
    {
        await vendorService.DeleteVendor(req.Id);
        await Send.OkAsync(cancellation: ct);
    }
}
