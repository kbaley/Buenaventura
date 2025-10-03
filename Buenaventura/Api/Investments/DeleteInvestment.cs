using Buenaventura.Services;
using FastEndpoints;

namespace Buenaventura.Api;

internal record DeleteInvestmentRequest(Guid InvestmentId);

internal class DeleteInvestment(IInvestmentService investmentService)
    : Endpoint<DeleteInvestmentRequest>
{
    public override void Configure()
    {
        Delete("/api/investments/{InvestmentId}");
    }

    public override async Task HandleAsync(DeleteInvestmentRequest req, CancellationToken ct)
    {
        await investmentService.DeleteInvestment(req.InvestmentId);
        await SendOkAsync(ct);
    }
}