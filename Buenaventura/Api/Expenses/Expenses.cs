using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class Expenses(IExpenseService expenseService)
    : EndpointWithoutRequest<IEnumerable<ReportDataPoint>>
{
    public override void Configure()
    {
        Get("/api/expenses");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var data = await expenseService.GetExpenseCategoryBreakdown();
        await SendOkAsync(data, ct);
    }
}