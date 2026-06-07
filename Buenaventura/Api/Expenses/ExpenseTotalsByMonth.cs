using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class ExpenseTotalsByMonth(IExpenseService expenseService)
    : Endpoint<ExpenseReportRequest, CategoryTotals>
{
    public override void Configure()
    {
        Get("/api/expenses/totalsbymonth");
    }

    public override async Task HandleAsync(ExpenseReportRequest req, CancellationToken ct)
    {
        var data = await expenseService.GetExpenseTotalsByMonth(
            TransactionTagFormatter.ParseTagText(req.IncludeTags),
            TransactionTagFormatter.ParseTagText(req.ExcludeTags),
            req.AllTime);
        await Send.OkAsync(data, ct);
    }
}
