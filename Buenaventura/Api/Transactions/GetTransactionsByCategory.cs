using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal record GetTransactionsByCategoryRequest(Guid CategoryId, int Page = 0, int PageSize = 50);

internal class GetTransactionsByCategory(ICategoryService categoryService) : Endpoint<GetTransactionsByCategoryRequest, TransactionListModel>
{
    public override void Configure()
    {
        Get($"/api/categories/{{CategoryId}}/transactions");
    }

    public override async Task HandleAsync(GetTransactionsByCategoryRequest request, CancellationToken ct)
    {
        var transactions = await categoryService.GetTransactions(request.CategoryId, request.Page, request.PageSize);
        await SendAsync(transactions, cancellation: ct);
    }
}