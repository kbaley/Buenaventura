using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Services;
using FastEndpoints;
using Newtonsoft.Json;

namespace Buenaventura.Api;

internal record GetExchangeRateRequest(string Symbol);

internal class GetExchangeRate(ICurrencyService currencyService) : Endpoint<GetExchangeRateRequest, decimal>
{
    public override void Configure()
    {
        Get("/api/currencies");
    }

    public override async Task HandleAsync(GetExchangeRateRequest req, CancellationToken ct)
    {
        var exchangeRate = await currencyService.GetExchangeRateFor(req.Symbol, ct);
        await SendAsync(exchangeRate, cancellation: ct);
    }
}
