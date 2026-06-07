using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

public class PutVendor(IVendorService vendorService) : Endpoint<VendorModel>
{
    public override void Configure()
    {
        Put("/api/vendors");
    }

    public override async Task HandleAsync(VendorModel req, CancellationToken ct)
    {
        await vendorService.UpdateVendor(req);
        await Send.OkAsync(cancellation: ct);
    }
}
