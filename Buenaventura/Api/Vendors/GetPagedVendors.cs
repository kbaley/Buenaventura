using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal sealed record GetPagedVendorsRequest(
    int Page = 0,
    int PageSize = 10,
    string? SortBy = "name",
    bool SortDescending = false);

internal class GetPagedVendors(IVendorService vendorService) : Endpoint<GetPagedVendorsRequest, PaginatedResults<VendorModel>>
{
    public override void Configure()
    {
        Get("/api/vendors/page");
    }

    public override async Task HandleAsync(GetPagedVendorsRequest req, CancellationToken ct)
    {
        var vendors = await vendorService.GetVendors(req.Page, req.PageSize, req.SortBy, req.SortDescending);
        await SendOkAsync(vendors, ct);
    }
}
