using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class ExpenseTotalsByMonth(IExpenseService expenseService)
    : EndpointWithoutRequest<CategoryTotals>
{
    public override void Configure()
    {
        Get("/api/expenses/totalsbymonth");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var data = await expenseService.GetExpenseTotalsByMonth();
        await SendOkAsync(data, ct);
    }
}