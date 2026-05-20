using Buenaventura.Domain;
using Buenaventura.Services;
using Buenaventura.Shared;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Buenaventura.Tests.Services;

public class ReimbursementServiceTests
{
    [Fact]
    public async Task GetReport_TreatsExpensesAsOutstandingAndRepaymentsAsReductions()
    {
        await using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var account = CreateAccount();
        var category = CreateCategory();
        context.Accounts.Add(account);
        context.Categories.Add(category);
        context.Transactions.AddRange(
            CreateTransaction(account, category, new DateTime(2026, 1, 10), -120m),
            CreateTransaction(account, category, new DateTime(2026, 1, 20), 50m),
            CreateTransaction(account, category, new DateTime(2026, 2, 5), -30m));
        await context.SaveChangesAsync();

        var service = new ReimbursementService(context);

        var report = await service.GetReport();

        report.Summary.OutstandingBalance.Should().Be(100m);
        report.Summary.OldestOutstandingDate.Should().Be(new DateTime(2026, 1, 10));
        report.MonthlyRows.Single(r => r.Month == new DateTime(2026, 1, 1)).RunningBalance.Should().Be(70m);
        report.MonthlyRows.Single(r => r.Month == new DateTime(2026, 2, 1)).RunningBalance.Should().Be(100m);
    }

    [Fact]
    public async Task CreateSettlement_RemovesClosedItemsFromOutstandingSummary()
    {
        await using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var account = CreateAccount();
        var category = CreateCategory();
        var expense = CreateTransaction(account, category, new DateTime(2026, 1, 10), -50m);
        var repayment = CreateTransaction(account, category, new DateTime(2026, 1, 20), 100m);
        var laterExpense = CreateTransaction(account, category, new DateTime(2026, 2, 5), -25m);
        context.Accounts.Add(account);
        context.Categories.Add(category);
        context.Transactions.AddRange(expense, repayment, laterExpense);
        await context.SaveChangesAsync();

        var service = new ReimbursementService(context);

        await service.CreateSettlement(new CreateReimbursementSettlementRequest
        {
            Name = "January settlement",
            TransactionIds = [expense.TransactionId, repayment.TransactionId],
            CloseImmediately = true
        });
        var report = await service.GetReport();

        report.Summary.OutstandingBalance.Should().Be(25m);
        report.Summary.OldestOutstandingDate.Should().Be(new DateTime(2026, 2, 5));
        report.UnsettledExpenses.Should().ContainSingle(t => t.TransactionId == laterExpense.TransactionId);
        report.UnsettledRepayments.Should().BeEmpty();
        report.Settlements.Should().ContainSingle(s =>
            s.Name == "January settlement" &&
            s.Expenses == 50m &&
            s.Repayments == 100m &&
            s.Difference == -50m);
    }

    private static Account CreateAccount()
    {
        return new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Card",
            AccountType = "Credit Card",
            Currency = "USD",
            Vendor = "",
            MortgageType = ""
        };
    }

    private static Category CreateCategory()
    {
        return new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = "To be reimbursed",
            Type = "Expense"
        };
    }

    private static Transaction CreateTransaction(Account account, Category category, DateTime date, decimal amount)
    {
        return new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = account.AccountId,
            Account = account,
            CategoryId = category.CategoryId,
            Category = category,
            TransactionDate = date,
            EnteredDate = date,
            Amount = amount,
            AmountInBaseCurrency = amount,
            TransactionType = Buenaventura.Shared.TransactionType.REGULAR
        };
    }
}
