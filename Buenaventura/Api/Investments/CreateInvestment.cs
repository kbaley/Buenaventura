using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class CreateInvestment(IInvestmentService investmentService)
    : Endpoint<AddInvestmentModel>
{
    public override void Configure()
    {
        Post("/api/investments");
    }

    public override async Task HandleAsync(AddInvestmentModel req, CancellationToken ct)
    {
        await investmentService.AddInvestment(req);
        await SendOkAsync(ct);
    }
}