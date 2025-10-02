using Buenaventura.Shared;
using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

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