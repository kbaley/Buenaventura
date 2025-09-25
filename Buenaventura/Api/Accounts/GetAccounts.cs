using Buenaventura.Client.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal class GetAccounts(IAccountService accountService) : EndpointWithoutRequest<List<AccountWithBalance>>
{
    public override void Configure()
    {
        Get("/api/accounts");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync((await accountService.GetAccounts()).ToList(), cancellation: ct);
    }
}