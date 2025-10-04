using Buenaventura.Services;
using FastEndpoints;

namespace Buenaventura.Api;

internal class LiquidAssetBalance(IDashboardService dashboardService)
    : EndpointWithoutRequest<decimal>
{
    public override void Configure()
    {
        Get("/api/dashboard/liquidassetbalance");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var balance = await dashboardService.GetLiquidAssetBalance();
        await SendOkAsync(balance, ct);
    }
}