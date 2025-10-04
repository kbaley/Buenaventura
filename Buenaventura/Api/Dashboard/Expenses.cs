using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class Expenses(IDashboardService dashboardService)
    : EndpointWithoutRequest<IEnumerable<ReportDataPoint>>
{
    public override void Configure()
    {
        Get("/api/dashboard/expenses");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var data = await dashboardService.GetExpenseCategoryBreakdown();
        await SendOkAsync(data, ct);
    }
}