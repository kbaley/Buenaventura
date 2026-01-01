using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class ExpenseTotalsByMonth(IDashboardService dashboardService)
    : EndpointWithoutRequest<CategoryTotals>
{
    public override void Configure()
    {
        Get("/api/expenses/totalsbymonth");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var data = await dashboardService.GetExpenseTotalsByMonth();
        await SendOkAsync(data, ct);
    }
}