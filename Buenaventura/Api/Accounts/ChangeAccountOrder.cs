using Buenaventura.Shared;
using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

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