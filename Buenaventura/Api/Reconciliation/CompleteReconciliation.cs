using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api.Reconciliation;

internal class CompleteReconciliation(IReconciliationService reconciliationService)
    : Endpoint<CompleteReconciliationRequest>
{
    public override void Configure()
    {
        Post("/api/accounts/{AccountId}/reconciliation/complete");
    }

    public override async Task HandleAsync(CompleteReconciliationRequest req, CancellationToken ct)
    {
        await reconciliationService.Complete(req);
        await SendOkAsync(ct);
    }
}
