using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api.Reimbursements;

internal class CreateReimbursementSettlement(IReimbursementService reimbursementService)
    : Endpoint<CreateReimbursementSettlementRequest>
{
    public override void Configure()
    {
        Post("/api/reimbursements/settlements");
    }

    public override async Task HandleAsync(CreateReimbursementSettlementRequest req, CancellationToken ct)
    {
        await reimbursementService.CreateSettlement(req);
        await Send.OkAsync(cancellation: ct);
    }
}
