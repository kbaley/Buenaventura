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

        var monthlyData = await expenseService.GetExpenseTotalsByMonth(req.CategoryId);
        var cutoff = DateTime.Today.AddMonths(-11);
        data.ThisMonthSpending = -monthExpenses;
        data.LastMonthSpending = -lastMonth;
        data.Category = await categoryService.GetCategory(req.CategoryId);
        data.ComparisonData = comparison;
        data.LastTwelveMonthsData = monthlyData
            .Where(x => x.Date >= cutoff)
            .Select(x => new ReportDataPoint { Label = x.Date.ToString("MMM yyyy"), Value = x.Amount })
            .ToList();
        data.PreviousTwelveMonthsData = monthlyData
            .Where(x => x.Date < cutoff)
            .Select(x => new ReportDataPoint { Label = x.Date.ToString("MMM yyyy"), Value = x.Amount })
            .ToList();
        data.VendorData = await expenseService.GetVendorSpendingByCategory(req.CategoryId);
        await SendOkAsync(data, ct);
    }
}