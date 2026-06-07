using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal sealed record GetVendorRequest(Guid Id);

internal class GetVendor(IVendorService vendorService) : Endpoint<GetVendorRequest, VendorModel>
{
    public override void Configure()
    {
        Get("/api/vendors/{Id}");
    }

    public override async Task HandleAsync(GetVendorRequest req, CancellationToken ct)
    {
        var vendor = await vendorService.GetVendor(req.Id);
        await Send.OkAsync(vendor, ct);
    }
}
