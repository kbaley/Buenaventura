using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class UpdateCurrentPrices(IInvestmentService investmentService)
    : EndpointWithoutRequest<InvestmentListModel>
{
    public override void Configure()
    {
        Post("/api/investments/updatecurrentprices");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(await investmentService.UpdateCurrentPrices(), ct);
    }
}