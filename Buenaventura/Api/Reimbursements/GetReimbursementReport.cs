using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api.Reimbursements;

internal class GetReimbursementReport(IReimbursementService reimbursementService)
    : EndpointWithoutRequest<ReimbursementReport>
{
    public override void Configure()
    {
        Get("/api/reimbursements/report");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var report = await reimbursementService.GetReport();
        await Send.OkAsync(report, ct);
    }
}
