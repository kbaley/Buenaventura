using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

internal sealed record GetExpenseCategoryPageDataRequest(Guid CategoryId, string? IncludeTags, string? ExcludeTags);

internal class GetExpenseCategoryPageData(IExpenseService expenseService, ICategoryService categoryService)
    : Endpoint<GetExpenseCategoryPageDataRequest, ExpenseCategoryPageData>
{
    public override void Configure()
    {
        Get("/api/expenses/category/{categoryId}");
    }

    public override async Task HandleAsync(GetExpenseCategoryPageDataRequest req, CancellationToken ct)
    {
        var includeTags = TransactionTagFormatter.ParseTagText(req.IncludeTags);
        var excludeTags = TransactionTagFormatter.ParseTagText(req.ExcludeTags);
        var monthExpenses = await expenseService.GetThisMonthExpenses(req.CategoryId, includeTags, excludeTags);
        var lastMonth = await expenseService.GetLastMonthExpenses(req.CategoryId, includeTags, excludeTags);
        var data = new ExpenseCategoryPageData();
        var averages = (await expenseService.GetExpenseAveragesData(req.CategoryId, includeTags, excludeTags)).FirstOrDefault();
        var comparison = new List<ReportDataPoint>();
        if (averages != null)
        {
            comparison.AddRange([
                new ReportDataPoint{ Value = averages.Last30Days, Label = "Last 30 Days"},
                new ReportDataPoint{ Value = averages.Last90DaysAverage, Label = "Last 90 Days"},
                new ReportDataPoint{ Value = averages.Last360DaysAverage, Label = "Last 360 Days"}
            ]);
        }

        var monthlyData = await expenseService.GetExpenseTotalsByMonth(req.CategoryId, includeTags, excludeTags);
        var cutoff = DateTime.Today.AddMonths(-12);
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
        data.VendorData = await expenseService.GetVendorSpending(req.CategoryId, includeTags, excludeTags);
        await Send.OkAsync(data, ct);
    }
}
