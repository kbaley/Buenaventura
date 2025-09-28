using Buenaventura.Client.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

public record ChangeAccountOrderRequest(List<OrderedAccount> Accounts);

public class ChangeAccountOrder(IAccountService accountService) : Endpoint<List<OrderedAccount>> 
{
    public override void Configure()
    {
        Post("api/accounts/order");
    }

    public override async Task HandleAsync(List<OrderedAccount> req, CancellationToken ct)
    {
        await accountService.SaveAccountOrder(req);
        await SendOkAsync(ct);
    }
}