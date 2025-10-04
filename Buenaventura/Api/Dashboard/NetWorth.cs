using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class NetWorth(IDashboardService dashboardService)
    : EndpointWithoutRequest<IEnumerable<ReportDataPoint>>
{
    public override void Configure()
    {
        Get("/api/dashboard/networth");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var data = await dashboardService.GetNetWorthData();
        await SendOkAsync(data, ct);
    }
}