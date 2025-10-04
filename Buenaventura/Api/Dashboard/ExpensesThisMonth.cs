using Buenaventura.Services;
using FastEndpoints;

namespace Buenaventura.Api;

internal class ExpensesThisMonth(IDashboardService dashboardService)
    : EndpointWithoutRequest<decimal>
{
    public override void Configure()
    {
        Get("/api/dashboard/expensesthismonth");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var total = await dashboardService.GetThisMonthExpenses();
        await SendOkAsync(total, ct);
    }
}