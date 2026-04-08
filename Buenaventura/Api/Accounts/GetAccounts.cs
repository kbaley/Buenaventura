using Buenaventura.Shared;
using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

internal sealed class GetAccountsRequest
{
    public bool IncludeHidden { get; set; }
}

internal class GetAccounts(IAccountService accountService) : Endpoint<GetAccountsRequest, List<AccountWithBalance>>
{
    public override void Configure()
    {
        Get("/api/accounts");
    }

    public override async Task HandleAsync(GetAccountsRequest req, CancellationToken ct)
    {
        await SendAsync((await accountService.GetAccounts(req.IncludeHidden)).ToList(), cancellation: ct);
    }
}
