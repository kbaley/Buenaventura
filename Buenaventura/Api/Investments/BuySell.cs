using Buenaventura.Shared;
using Buenaventura.Services;
using FastEndpoints;

namespace Buenaventura.Api;

internal class BuySell(IInvestmentService investmentService)
    : Endpoint<BuySellModel>
{
    public override void Configure()
    {
        Post("/api/investments/buysell");
    }

    public override async Task HandleAsync(BuySellModel req, CancellationToken ct)
    {
        await investmentService.BuySell(req);
        await SendOkAsync(ct);
    }
}