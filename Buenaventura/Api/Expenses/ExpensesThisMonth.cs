using Buenaventura.Services;
using FastEndpoints;

namespace Buenaventura.Api;

internal class ExpensesThisMonth(IExpenseService expenseService)
    : EndpointWithoutRequest<decimal>
{
    public override void Configure()
    {
        Get("/api/expenses/thismonth");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var total = await expenseService.GetThisMonthExpenses();
        await SendOkAsync(total, ct);
    }
}