using Buenaventura.Shared;
using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

internal record GetAllAccountsTransactionsRequest(DateTime Start, DateTime End);

internal class GetAllAccountsTransactions(IAccountService accountService) : Endpoint<GetAllAccountsTransactionsRequest, TransactionListModel>
{
    public override void Configure()
    {
        Get("/api/transactions/all");
    }

    public override async Task HandleAsync(GetAllAccountsTransactionsRequest request, CancellationToken ct)
    {
        var transactions = await accountService.GetAllTransactions(request.Start, request.End);
        await SendAsync(transactions, cancellation: ct);
    }
}
