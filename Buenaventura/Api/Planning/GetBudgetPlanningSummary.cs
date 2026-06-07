using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class GetBudgetPlanningSummary(IBudgetPlanningService planningService)
    : EndpointWithoutRequest<BudgetPlanningSummary>
{
    public override void Configure()
    {
        Get("/api/planning/budget");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var data = await planningService.GetBudgetPlanningSummary();
        await Send.OkAsync(data, ct);
    }
}
