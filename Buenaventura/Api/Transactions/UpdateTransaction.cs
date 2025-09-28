using Buenaventura.Client.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

public class UpdateTransaction(IAccountService accountService) : Endpoint<TransactionForDisplay>
{
    
    public override void Configure()
    {
        Put("/api/transactions/{TransactionId}");
    }

    public override async Task HandleAsync(TransactionForDisplay req, CancellationToken ct)
    {
        await accountService.UpdateTransaction(req);
        await SendOkAsync(ct);
    }
}