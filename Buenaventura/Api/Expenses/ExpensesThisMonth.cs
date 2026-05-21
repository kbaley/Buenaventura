using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class ExpensesThisMonth(IExpenseService expenseService)
    : Endpoint<ExpenseReportRequest, decimal>
{
    public override void Configure()
    {
        Get("/api/expenses/thismonth");
    }

    public override async Task HandleAsync(ExpenseReportRequest req, CancellationToken ct)
    {
        var total = await expenseService.GetThisMonthExpenses(
            includeTags: TransactionTagFormatter.ParseTagText(req.IncludeTags),
            excludeTags: TransactionTagFormatter.ParseTagText(req.ExcludeTags));
        await SendOkAsync(total, ct);
    }
}
