using Buenaventura.Services.Retirement;
using Buenaventura.Shared.Retirement;
using FastEndpoints;

namespace Buenaventura.Api.Retirement;

internal class Ask(IRetirementAdvisorService advisor) : Endpoint<RetirementQueryRequest, RetirementQueryResponse>
{
    public override void Configure()
    {
        Post("/api/retirement/ask");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RetirementQueryRequest req, CancellationToken ct)
    {
        var result = await advisor.AskAsync(req, ct);
        await SendOkAsync(result, ct);
    }
}
