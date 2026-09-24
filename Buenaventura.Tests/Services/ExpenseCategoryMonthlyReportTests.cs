using Buenaventura.Client.Services;
using Buenaventura.Shared;
using FluentAssertions;
using Xunit;

namespace Buenaventura.Tests.Services;

public class ExpenseCategoryMonthlyReportTests
{
    [Fact]
    public void GetVisibleCategories_UsesOnlyShownMonthsAndOmitsNetZeroCategories()
    {
        var categories = new[]
        {
            Category("Empty"),
            Category("Zero", new MonthlyAmount(2026, 1, 0)),
            Category("Refunded", new MonthlyAmount(2026, 1, 100), new MonthlyAmount(2026, 2, -100)),
            Category("Outside period", new MonthlyAmount(2025, 1, 200)),
            Category("Expense", new MonthlyAmount(2026, 1, 50), new MonthlyAmount(2025, 1, 300)),
            Category("Net refund", new MonthlyAmount(2026, 2, -25))
        };

        var result = ExpenseCategoryMonthlyReport.GetVisibleCategories(categories,
            [new DateTime(2026, 1, 24), new DateTime(2026, 2, 24)]);

        result.Select(c => c.CategoryName).Should().Equal("Expense", "Net refund");
        result.Select(c => c.Total).Should().Equal(50m, -25m);
        result[0].Amounts.Should().ContainSingle().Which.Amount.Should().Be(50m);
        categories[4].Amounts.Should().HaveCount(2);
        categories[4].Total.Should().Be(350m);
    }

    [Fact]
    public void GetVisibleCategories_AllTimePeriodIncludesOlderActivity()
    {
        var category = Category("Project", new MonthlyAmount(2020, 1, 100), new MonthlyAmount(2026, 2, 50));

        var result = ExpenseCategoryMonthlyReport.GetVisibleCategories([category],
            [new DateTime(2020, 1, 1), new DateTime(2026, 2, 1)]);

        result.Should().ContainSingle().Which.Total.Should().Be(150m);
    }

    private static CategoryTotal Category(string name, params MonthlyAmount[] amounts) => new()
    {
        CategoryId = Guid.NewGuid(),
        CategoryName = name,
        Amounts = amounts,
        Total = amounts.Sum(a => a.Amount)
    };
}
