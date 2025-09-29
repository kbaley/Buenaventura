using Buenaventura.Client.Services;
using Buenaventura.Shared;
using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

public class CreateTransaction(IAccountService accountService) : Endpoint<TransactionForDisplay>
{
    public override void Configure()
    {
        Post("/api/accounts/{AccountId}/transactions");
    }

    public override async Task HandleAsync(TransactionForDisplay req, CancellationToken ct)
    {
        await accountService.AddTransaction(req.AccountId!.Value, req);
        await SendOkAsync(ct);
    }
}