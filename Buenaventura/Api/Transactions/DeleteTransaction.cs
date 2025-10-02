using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

public record DeleteTransactionRequest(Guid Id);

public class DeleteTransaction(IAccountService accountService) : Endpoint<DeleteTransactionRequest>
{
    public override void Configure()
    {
        Delete("/api/transactions/{id}");
    }

    public override async Task HandleAsync(DeleteTransactionRequest req, CancellationToken ct)
    {
        await accountService.DeleteTransaction(req.Id);
        await SendOkAsync(ct);
    }
}