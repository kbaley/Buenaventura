using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class Investments(IDashboardService dashboardService)
    : EndpointWithoutRequest<IEnumerable<ReportDataPoint>>
{
    public override void Configure()
    {
        Get("/api/dashboard/investments");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var data = await dashboardService.GetInvestmentData();
        await SendOkAsync(data, ct);
    }
}