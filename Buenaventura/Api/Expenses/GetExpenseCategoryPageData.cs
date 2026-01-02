using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal sealed record GetExpenseCategoryPageDataRequest(Guid CategoryId);

internal class GetExpenseCategoryPageData(IExpenseService expenseService, ICategoryService categoryService)
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
        var averages = (await expenseService.GetExpenseAveragesData(req.CategoryId)).FirstOrDefault();
        var comparison = new List<ReportDataPoint>();
        if (averages != null)
        {
            comparison.AddRange([
                new ReportDataPoint{ Value = averages.Last30Days, Label = "Last 30 Days"},
                new ReportDataPoint{ Value = averages.Last90DaysAverage, Label = "Last 90 Days"},
                new ReportDataPoint{ Value = averages.Last360DaysAverage, Label = "Last 360 Days"}
            ]);
        }
        data.ThisMonthSpending = -monthExpenses;
        data.LastMonthSpending = -lastMonth;
        data.Category = await categoryService.GetCategory(req.CategoryId);
        data.ComparisonData = comparison;
        await SendOkAsync(data, ct);
    }
}