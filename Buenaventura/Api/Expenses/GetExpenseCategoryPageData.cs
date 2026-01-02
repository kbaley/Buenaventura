using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal sealed record GetExpenseCategoryPageDataRequest(Guid CategoryId);

internal class GetExpenseCategoryPageData(IExpenseService expenseService)
    : Endpoint<GetExpenseCategoryPageDataRequest, ExpenseCategoryPageData>
{
    public override void Configure()
    {
        Get("/api/expenses/category/{categoryId}");
    }

    public override async Task HandleAsync(GetExpenseCategoryPageDataRequest req, CancellationToken ct)
    {
        var monthExpenses = await expenseService.GetThisMonthExpenses(req.CategoryId);
        var lastMonth = await expenseService.GetLastMonthExpenses(req.CategoryId);
        var data = new ExpenseCategoryPageData();
        data.ThisMonthSpending = -monthExpenses;
        data.LastMonthSpending = -lastMonth;
        await SendOkAsync(data, ct);
    }
}