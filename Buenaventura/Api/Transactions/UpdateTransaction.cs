using Buenaventura.Shared;
using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

public class UpdateTransaction(IAccountService accountService) : Endpoint<TransactionForDisplay>
{
    
    public override void Configure()
    {
        Put("/api/transactions");
    }

    public override async Task HandleAsync(TransactionForDisplay req, CancellationToken ct)
    {
        await accountService.UpdateTransaction(req);
        await SendOkAsync(ct);
    }
}