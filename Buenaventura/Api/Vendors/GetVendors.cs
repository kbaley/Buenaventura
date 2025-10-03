using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

public class GetVendors(IVendorService vendorService) : EndpointWithoutRequest<IEnumerable<VendorModel>>
{
    public override void Configure()
    {
        Get("/api/vendors");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var vendors = await vendorService.GetVendors();
        await SendOkAsync(vendors, ct);
    }
}