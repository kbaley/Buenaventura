using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api.Reconciliation;

internal record GetReconciliationWorkspaceRequest(Guid AccountId, DateTime? AsOfDate = null);

internal class GetReconciliationWorkspace(IReconciliationService reconciliationService)
    : Endpoint<GetReconciliationWorkspaceRequest, ReconciliationWorkspace>
{
    public override void Configure()
    {
        Get("/api/accounts/{AccountId}/reconciliation");
    }

    public override async Task HandleAsync(GetReconciliationWorkspaceRequest req, CancellationToken ct)
    {
        var workspace = await reconciliationService.GetWorkspace(req.AccountId, req.AsOfDate ?? DateTime.Today);
        await Send.OkAsync(workspace, cancellation: ct);
    }
}
