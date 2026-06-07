using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class GetVendorData(IExpenseService expenseService)
    : Endpoint<ExpenseReportRequest, List<ReportDataPoint>>
{
    public override void Configure()
    {
        Get("/api/expenses/vendors");
    }

    public override async Task HandleAsync(ExpenseReportRequest req, CancellationToken ct)
    {
        // Ignore "Other" category for total vendor spending
        var data = (await expenseService.GetVendorSpending(
                includeTags: TransactionTagFormatter.ParseTagText(req.IncludeTags),
                excludeTags: TransactionTagFormatter.ParseTagText(req.ExcludeTags),
                allTime: req.AllTime))
            .Where(d => d.Label != "Other")
            .ToList();
        await Send.OkAsync(data, ct);
    }
}
