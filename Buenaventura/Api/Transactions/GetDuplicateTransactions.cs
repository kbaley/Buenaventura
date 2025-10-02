using Buenaventura.Shared;
using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

internal record GetDuplicateTransactionsRequest(Guid AccountId);

internal class GetDuplicateTransactions(IAccountService accountService) : Endpoint<GetDuplicateTransactionsRequest, TransactionListModel>
{
    public override void Configure()
    {
        Get($"/api/accounts/{{AccountId}}/transactions/duplicates");
    }

    public override async Task HandleAsync(GetDuplicateTransactionsRequest request, CancellationToken ct)
    {
        var transactions = await accountService.GetPotentialDuplicateTransactions(request.AccountId);
        await SendAsync(transactions, cancellation: ct);
    }
}