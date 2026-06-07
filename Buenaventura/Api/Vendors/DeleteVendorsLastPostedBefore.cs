using Buenaventura.Services;
using FastEndpoints;

namespace Buenaventura.Api;

internal sealed record DeleteVendorsLastPostedBeforeRequest(DateTime LastPostedBefore);

internal class DeleteVendorsLastPostedBefore(IVendorService vendorService)
    : Endpoint<DeleteVendorsLastPostedBeforeRequest, int>
{
    public override void Configure()
    {
        Delete("/api/vendors");
    }

    public override async Task HandleAsync(DeleteVendorsLastPostedBeforeRequest req, CancellationToken ct)
    {
        var deletedCount = await vendorService.DeleteVendorsLastPostedBefore(req.LastPostedBefore);
        await Send.OkAsync(deletedCount, ct);
    }
}
