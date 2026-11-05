using Buenaventura.Shared;
using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

public class BulkRemoveTagTransactions(IAccountService accountService) : Endpoint<BulkTagTransactionsRequest, int>
{
    public override void Configure()
    {
        Put("/api/accounts/{AccountId}/transactions/bulk-remove-tag");
    }

    public override async Task HandleAsync(BulkTagTransactionsRequest req, CancellationToken ct)
    {
        var updatedCount = await accountService.BulkRemoveTagTransactions(req.AccountId, req.TransactionIds, req.Tag);
        await Send.OkAsync(updatedCount, ct);
    }
}
