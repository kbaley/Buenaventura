using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class IncomeExpenses(IDashboardService dashboardService)
    : EndpointWithoutRequest<IEnumerable<IncomeExpenseDataPoint>>
{
    public override void Configure()
    {
        Get("/api/dashboard/incomeexpenses");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var data = await dashboardService.GetIncomeExpenseData();
        await SendOkAsync(data, ct);
    }
}