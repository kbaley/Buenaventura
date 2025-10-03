using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class GetInvestments(IInvestmentService investmentService) : EndpointWithoutRequest<InvestmentListModel>
{
    public override void Configure()
    {
        Get("/api/investments");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(await investmentService.GetInvestments(), ct);
    }
}