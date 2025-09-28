using Buenaventura.Client.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

public class UpdateAccount(IAccountService accountService) : Endpoint<AccountWithBalance>
{
    public override void Configure()
    {
        Put("api/accounts");
    }

    public override async Task HandleAsync(AccountWithBalance req, CancellationToken ct)
    {
        await accountService.UpdateAccount(req);
        await SendOkAsync(ct);
    }
}