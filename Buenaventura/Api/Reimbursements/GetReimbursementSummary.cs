using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api.Reimbursements;

internal class GetReimbursementSummary(IReimbursementService reimbursementService)
    : EndpointWithoutRequest<ReimbursementSummary>
{
    public override void Configure()
    {
        Get("/api/reimbursements/summary");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var summary = await reimbursementService.GetSummary();
        await SendOkAsync(summary, ct);
    }
}
