using Buenaventura.Data;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal record GetTransactionsByTagsRequest(string? IncludeTags, string? ExcludeTags, int Page = 0, int PageSize = 50);

internal class GetTransactionsByTags(ITransactionRepository transactionRepository) : Endpoint<GetTransactionsByTagsRequest, TransactionListModel>
{
    public override void Configure()
    {
        Get("/api/transactions/tags");
    }

    public override async Task HandleAsync(GetTransactionsByTagsRequest request, CancellationToken ct)
    {
        var transactions = await transactionRepository.GetByTags(
            TransactionTagFormatter.ParseTagText(request.IncludeTags),
            TransactionTagFormatter.ParseTagText(request.ExcludeTags),
            request.Page,
            request.PageSize);
        await Send.OkAsync(transactions, cancellation: ct);
    }
}
