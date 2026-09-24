using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Services;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Buenaventura.Tests.Services;

public class ExpenseServiceTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public async Task GetExpenseTotalsByMonth_FiltersCategoriesAndTotalsForRestrictedUsers(bool isRestricted, bool allTime)
    {
        await using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var allowed = CreateCategory("Allowed");
        var excluded = CreateCategory("Excluded", true);
        var allowedEmpty = CreateCategory("Allowed empty");
        var excludedEmpty = CreateCategory("Excluded empty", true);
        context.Categories.AddRange(allowed, excluded, allowedEmpty, excludedEmpty);

        var month = DateTime.Today.FirstDayOfMonth().AddMonths(-1);
        AddExpense(context, allowed, month.AddDays(2), -100m);
        AddExpense(context, allowed, month.AddMonths(-1).AddDays(2), -50m);
        AddExpense(context, excluded, month.AddDays(2), -900m);
        AddExpense(context, excluded, month.AddMonths(-1).AddDays(2), -450m);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, Mock.Of<IReportRepository>());
        var result = await service.GetExpenseTotalsByMonth(allTime: allTime, isRestricted: isRestricted);

        var expectedIds = isRestricted
            ? new[] { allowed.CategoryId, allowedEmpty.CategoryId }
            : new[] { allowed.CategoryId, allowedEmpty.CategoryId, excluded.CategoryId, excludedEmpty.CategoryId };
        result.Expenses.Select(c => c.CategoryId).Should().BeEquivalentTo(expectedIds);
        result.Expenses.Single(c => c.CategoryId == allowed.CategoryId).Total.Should().Be(150m);
        result.Total.Should().Be(isRestricted ? 150m : 1500m);
        result.MonthTotals.Should().NotBeNull();
        result.MonthTotals!.Single(m => m.Date == month).Amount.Should().Be(isRestricted ? 100m : 1000m);
        result.MonthTotals.Single(m => m.Date == month.AddMonths(-1)).Amount.Should().Be(isRestricted ? 50m : 500m);
        result.MonthTotals.Sum(m => m.Amount).Should().Be(result.Total);
        result.Total.Should().Be(result.Expenses.Sum(c => c.Total));
    }

    [Fact]
    public async Task GetExpenseTotalsByMonth_WhenAllCategoriesAreExcluded_ReturnsEmptyReport()
    {
        await using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var excluded = CreateCategory("Excluded", true);
        context.Categories.Add(excluded);
        AddExpense(context, excluded, DateTime.Today.FirstDayOfMonth().AddDays(-2), -100m);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, Mock.Of<IReportRepository>());
        var result = await service.GetExpenseTotalsByMonth(allTime: true, isRestricted: true);

        result.Expenses.Should().BeEmpty();
        result.Total.Should().Be(0m);
        result.MonthTotals.Should().OnlyContain(m => m.Amount == 0m);
        result.StartDate.Should().BeNull();
        result.EndDate.Should().BeNull();
    }

    private static Category CreateCategory(string name, bool excluded = false) => new()
    {
        CategoryId = Guid.NewGuid(),
        Name = name,
        Type = "Expense",
        ExcludeFromTransactionReport = excluded
    };

    private static void AddExpense(BuenaventuraDbContext context, Category category, DateTime date, decimal amount)
    {
        context.Transactions.Add(new Transaction
        {
            TransactionId = Guid.NewGuid(),
            CategoryId = category.CategoryId,
            Category = category,
            TransactionDate = date,
            Amount = amount,
            AmountInBaseCurrency = amount
        });
    }
}
