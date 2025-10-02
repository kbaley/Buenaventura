using Buenaventura.Shared;
using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

internal record GetAccountRequest(Guid AccountId);
internal class GetAccount(IAccountService accountService) : Endpoint<GetAccountRequest, AccountWithBalance>
{
    public override void Configure()
    {
        Get("/api/accounts/{AccountId}");
    }

    public override async Task HandleAsync(GetAccountRequest request, CancellationToken ct)
    {
        var account = await accountService.GetAccount(request.AccountId);
        await SendAsync(account, cancellation: ct);
    }
}