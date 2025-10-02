using Buenaventura.Shared;
using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

internal record GetAllTransactionsRequest(Guid AccountId, DateTime Start, DateTime End);

internal class GetAllTransactions(IAccountService accountService) : Endpoint<GetAllTransactionsRequest, TransactionListModel>
{
    public override void Configure()
    {
        Get("/api/accounts/{AccountId}/transactions/all");
    }

    public override async Task HandleAsync(GetAllTransactionsRequest request, CancellationToken ct)
    {
        // Get all transactions without pagination for duplicate checking
        var transactions = await accountService.GetAllTransactions(request.AccountId, request.Start, request.End); 
        await SendAsync(transactions, cancellation: ct);
    }
}