using Buenaventura.Services;
using FastEndpoints;

namespace Buenaventura.Api;

internal class MakeCorrectingEntry(IInvestmentService investmentService)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/api/investments/makecorrectingentry");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await investmentService.MakeCorrectingEntry();
        await SendOkAsync(ct);
    }
}