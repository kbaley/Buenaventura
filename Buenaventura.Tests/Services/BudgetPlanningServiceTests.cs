using Buenaventura.Domain;
using Buenaventura.Services;
using Buenaventura.Shared;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Buenaventura.Tests.Services;

public class BudgetPlanningServiceTests
{
    [Fact]
    public async Task GetBudgetPlanningSummary_DetectsMonthlyRecurringExpenses()
    {
        await using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var account = CreateAccount();
        var subscriptionCategory = CreateExpenseCategory("Subscriptions");
        var groceriesCategory = CreateExpenseCategory("Groceries");
        context.Accounts.Add(account);
        context.Categories.AddRange(subscriptionCategory, groceriesCategory);

        var currentMonthStart = DateTime.Today.FirstDayOfMonth();
        for (var i = 1; i <= 5; i++)
        {
            AddExpense(context, account, subscriptionCategory, currentMonthStart.AddMonths(-i).AddDays(4), "Streamly", -14.99m);
        }

        AddExpense(context, account, groceriesCategory, currentMonthStart.AddMonths(-1).AddDays(2), "Market", -87.12m);
        AddExpense(context, account, groceriesCategory, currentMonthStart.AddMonths(-2).AddDays(10), "Different Market", -115.34m);

        await context.SaveChangesAsync();

        var service = new BudgetPlanningService(context);

        var summary = await service.GetBudgetPlanningSummary();

        summary.RecurringExpenses.Should().ContainSingle();
        var recurringExpense = summary.RecurringExpenses.Single();
        recurringExpense.Vendor.Should().Be("Streamly");
        recurringExpense.CategoryName.Should().Be("Subscriptions");
        recurringExpense.AverageAmount.Should().Be(14.99m);
        recurringExpense.Cadence.Should().Be("Monthly");
        summary.RecurringMonthlyTotal.Should().Be(14.99m);
    }

    [Fact]
    public async Task GetBudgetPlanningSummary_GeneratesCategoryBudgetsFromCompletedMonths()
    {
        await using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var account = CreateAccount();
        var diningCategory = CreateExpenseCategory("Dining");
        context.Accounts.Add(account);
        context.Categories.Add(diningCategory);

        var currentMonthStart = DateTime.Today.FirstDayOfMonth();
        for (var i = 1; i <= 12; i++)
        {
            AddExpense(context, account, diningCategory, currentMonthStart.AddMonths(-i).AddDays(5), "Cafe", -100m);
        }

        AddExpense(context, account, diningCategory, currentMonthStart.AddDays(3), "Cafe", -130m);

        await context.SaveChangesAsync();

        var service = new BudgetPlanningService(context);

        var summary = await service.GetBudgetPlanningSummary();

        var diningBudget = summary.BudgetCategories.Should().ContainSingle().Subject;
        diningBudget.CategoryName.Should().Be("Dining");
        diningBudget.AverageMonthlySpend.Should().Be(100m);
        diningBudget.MedianMonthlySpend.Should().Be(100m);
        diningBudget.SuggestedMonthlyBudget.Should().Be(100m);
        diningBudget.CurrentMonthSpend.Should().Be(130m);
        diningBudget.IsCurrentMonthHigherThanBudget.Should().BeTrue();
    }

    private static Account CreateAccount()
    {
        return new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Checking",
            Currency = "USD",
            Vendor = "",
            AccountType = "Bank Account",
            MortgageType = "",
            Transactions = new List<Transaction>()
        };
    }

    private static Category CreateExpenseCategory(string name)
    {
        return new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = name,
            Type = "Expense",
            IncludeInReports = true
        };
    }

    private static void AddExpense(
        Buenaventura.Data.BuenaventuraDbContext context,
        Account account,
        Category category,
        DateTime date,
        string vendor,
        decimal amount)
    {
        context.Transactions.Add(new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = account.AccountId,
            Account = account,
            CategoryId = category.CategoryId,
            Category = category,
            Vendor = vendor,
            Description = vendor,
            Amount = amount,
            AmountInBaseCurrency = amount,
            TransactionDate = date,
            TransactionType = TransactionType.REGULAR
        });
    }
}
