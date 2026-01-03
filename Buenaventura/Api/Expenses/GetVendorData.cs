using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class GetVendorData(IExpenseService expenseService)
    : EndpointWithoutRequest<List<ReportDataPoint>>
{
    public override void Configure()
    {
        Get("/api/expenses/vendors");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Ignore "Other" category for total vendor spending
        var data = (await expenseService.GetVendorSpending())
            .Where(d => d.Label != "Other")
            .ToList();
        await SendOkAsync(data, ct);
    }
}