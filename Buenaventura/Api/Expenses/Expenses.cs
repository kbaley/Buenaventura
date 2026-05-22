using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal sealed record ExpenseReportRequest(string? IncludeTags, string? ExcludeTags, bool AllTime = false);

internal class Expenses(IExpenseService expenseService)
    : Endpoint<ExpenseReportRequest, IEnumerable<ReportDataPoint>>
{
    public override void Configure()
    {
        Get("/api/expenses");
    }

    public override async Task HandleAsync(ExpenseReportRequest req, CancellationToken ct)
    {
        var data = await expenseService.GetExpenseCategoryBreakdown(
            TransactionTagFormatter.ParseTagText(req.IncludeTags),
            TransactionTagFormatter.ParseTagText(req.ExcludeTags),
            req.AllTime);
        await SendOkAsync(data, ct);
    }
}
