using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class AssetClasses(IDashboardService dashboardService)
    : EndpointWithoutRequest<IEnumerable<ReportDataPoint>>
{
    public override void Configure()
    {
        Get("/api/dashboard/assetclasses");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var data = await dashboardService.GetAssetClassData();
        await SendOkAsync(data, ct);
    }
}