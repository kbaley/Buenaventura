using Buenaventura.Client.Services;
using Buenaventura.Shared;
using FastEndpoints;

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