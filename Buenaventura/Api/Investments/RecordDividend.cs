using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class RecordDividend(IInvestmentService investmentService)
    : Endpoint<RecordDividendModel>
{
    public override void Configure()
    {
        Post("/api/investments/dividends");
    }

    public override async Task HandleAsync(RecordDividendModel req, CancellationToken ct)
    {
        await investmentService.RecordDividend(req.InvestmentId, req);
        await SendOkAsync(ct);
    }
}