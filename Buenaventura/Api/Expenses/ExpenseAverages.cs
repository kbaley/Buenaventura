using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class ExpenseAverages(IDashboardService dashboardService)
    : EndpointWithoutRequest<IEnumerable<ExpenseAveragesDataPoint>>
{
    public override void Configure()
    {
        Get("/api/expenses/averages");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var data = await dashboardService.GetExpenseAveragesData();
        await SendOkAsync(data, ct);
    }
}