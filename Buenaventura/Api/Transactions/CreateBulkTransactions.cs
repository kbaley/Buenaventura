using Buenaventura.Client.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

public record CreateBulkTransactionsRequest(Guid AccountId, List<TransactionForDisplay> Transactions);

public class CreateBulkTransactions(IAccountService accountService) : Endpoint<CreateBulkTransactionsRequest>
{
    public override void Configure()
    {
        Post("/api/accounts/{AccountId}/transactions/bulk");
    }

    public override async Task HandleAsync(CreateBulkTransactionsRequest req, CancellationToken ct)
    {
        await accountService.AddBulkTransactions(req.AccountId, req.Transactions);
        await SendOkAsync(ct);
    }
}