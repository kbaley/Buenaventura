using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public static class ExpenseCategoryMonthlyReport
{
    public static List<CategoryTotal> GetVisibleCategories(IEnumerable<CategoryTotal> categories, IEnumerable<DateTime> months)
    {
        var visibleMonths = months.Select(m => (m.Year, m.Month)).ToHashSet();
        return categories.Select(category =>
            {
                var amounts = category.Amounts.Where(a => visibleMonths.Contains((a.Date.Year, a.Date.Month))).ToList();
                return new CategoryTotal
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    Amounts = amounts,
                    Total = amounts.Sum(a => a.Amount)
                };
            })
            .Where(category => category.Total != 0m)
            .ToList();
    }
}
