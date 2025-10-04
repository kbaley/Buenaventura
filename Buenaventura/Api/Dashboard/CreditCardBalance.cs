using Buenaventura.Services;
using FastEndpoints;

namespace Buenaventura.Api;

internal class CreditCardBalance(IDashboardService dashboardService)
    : EndpointWithoutRequest<decimal>
{
    public override void Configure()
    {
        Get("/api/dashboard/creditcardbalance");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var balance = await dashboardService.GetCreditCardBalance();
        await SendOkAsync(balance, ct);
    }
}