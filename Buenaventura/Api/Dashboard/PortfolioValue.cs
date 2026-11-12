using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class PortfolioValue(IDashboardService dashboardService)
    : EndpointWithoutRequest<IEnumerable<ReportDataPoint>>
{
    public override void Configure()
    {
        Get("/api/dashboard/portfoliovalue");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var data = await dashboardService.GetPortfolioValueData();
        await Send.OkAsync(data, ct);
    }
}