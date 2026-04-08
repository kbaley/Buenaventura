using Buenaventura.Shared;
using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

internal sealed record DeleteAccountRequest(Guid Id);

internal sealed class DeleteAccount(IAccountService accountService) : Endpoint<DeleteAccountRequest, DeleteAccountResponse>
{
    public override void Configure()
    {
        Delete("/api/accounts/{Id}");
    }

    public override async Task HandleAsync(DeleteAccountRequest req, CancellationToken ct)
    {
        var result = await accountService.DeleteAccount(req.Id);
        await SendAsync(result, result.Success ? 200 : 400, ct);
    }
}
