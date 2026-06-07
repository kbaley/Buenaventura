using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class ExpenseAverages(IExpenseService expenseService)
    : Endpoint<ExpenseReportRequest, IEnumerable<ExpenseAveragesDataPoint>>
{
    public override void Configure()
    {
        Get("/api/expenses/averages");
    }

    public override async Task HandleAsync(ExpenseReportRequest req, CancellationToken ct)
    {
        var data = await expenseService.GetExpenseAveragesData(
            includeTags: TransactionTagFormatter.ParseTagText(req.IncludeTags),
            excludeTags: TransactionTagFormatter.ParseTagText(req.ExcludeTags));
        await Send.OkAsync(data, ct);
    }
}
