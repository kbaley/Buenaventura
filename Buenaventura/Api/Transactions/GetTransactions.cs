using Buenaventura.Client.Services;
using Buenaventura.Shared;
using FastEndpoints;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

internal record GetAccountTransactionsRequest(Guid AccountId, int Page = 0, int PageSize = 50, string? Search = null);

internal class GetTransactions(IAccountService accountService) : Endpoint<GetAccountTransactionsRequest, TransactionListModel>
{
    public override void Configure()
    {
        Get($"/api/accounts/{{AccountId}}/transactions");
    }

    public override async Task HandleAsync(GetAccountTransactionsRequest request, CancellationToken ct)
    {
        var transactions = await accountService
            .GetTransactions(request.AccountId, request.Search ?? "", request.Page, request.PageSize);
        await SendAsync(transactions, cancellation: ct);
    }
}