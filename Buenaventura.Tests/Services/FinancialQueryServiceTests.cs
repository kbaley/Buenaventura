using Buenaventura.Domain;
using Buenaventura.Services;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Buenaventura.Tests.Services;

public sealed class FinancialQueryServiceTests
{
    [Fact]
    public async Task GetSummary_ReturnsPositiveIncomeAndExpenseAmountsByCategory()
    {
        await using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var account = TestDataFactory.AccountFaker.Generate();
        var expenseCategory = new Category { CategoryId = Guid.NewGuid(), Name = "Groceries", Type = "Expense" };
        var incomeCategory = new Category { CategoryId = Guid.NewGuid(), Name = "Salary", Type = "Income" };
        context.AddRange(account, expenseCategory, incomeCategory);
        context.Transactions.AddRange(
            CreateTransaction(account, expenseCategory, new DateTime(2026, 5, 10), -42.50m),
            CreateTransaction(account, incomeCategory, new DateTime(2026, 5, 15), 2_000m),
            CreateTransaction(account, expenseCategory, new DateTime(2026, 4, 30), -99m));
        await context.SaveChangesAsync();

        var results = await new FinancialQueryService(context).GetSummary(
            new DateTime(2026, 5, 1),
            new DateTime(2026, 6, 1),
            null);

        results.Should().BeEquivalentTo([
            new { Type = "Expense", Category = "Groceries", AmountInBaseCurrency = 42.50m, EntryCount = 1 },
            new { Type = "Income", Category = "Salary", AmountInBaseCurrency = 2_000m, EntryCount = 1 }
        ]);
    }

    [Fact]
    public async Task GetTransactions_AppliesTypeAndResultLimit()
    {
        await using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var account = TestDataFactory.AccountFaker.Generate();
        var expenseCategory = new Category { CategoryId = Guid.NewGuid(), Name = "Dining", Type = "Expense" };
        var incomeCategory = new Category { CategoryId = Guid.NewGuid(), Name = "Salary", Type = "Income" };
        context.AddRange(account, expenseCategory, incomeCategory);
        context.Transactions.AddRange(
            CreateTransaction(account, expenseCategory, new DateTime(2026, 5, 10), -20m),
            CreateTransaction(account, expenseCategory, new DateTime(2026, 5, 20), -30m),
            CreateTransaction(account, incomeCategory, new DateTime(2026, 5, 25), 2_000m));
        await context.SaveChangesAsync();

        var results = await new FinancialQueryService(context).GetTransactions(
            new DateTime(2026, 5, 1),
            new DateTime(2026, 6, 1),
            "expense",
            null,
            1);

        results.Should().ContainSingle();
        results[0].Date.Should().Be(new DateTime(2026, 5, 20));
        results[0].AmountInBaseCurrency.Should().Be(30m);
    }

    private static Transaction CreateTransaction(
        Account account,
        Category category,
        DateTime date,
        decimal amount) => new()
    {
        TransactionId = Guid.NewGuid(),
        AccountId = account.AccountId,
        Account = account,
        CategoryId = category.CategoryId,
        Category = category,
        TransactionDate = date,
        EnteredDate = date,
        Amount = amount,
        AmountInBaseCurrency = amount
    };
}
