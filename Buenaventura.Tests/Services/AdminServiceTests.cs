using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Services;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Buenaventura.Tests.Services;

public class AdminServiceTests
{
    [Fact]
    public async Task ResetDemoDatabase_ReplacesFinancialDataAndPreservesUsers()
    {
        await using var context = TestDbContextFactory.CreateSqliteDbContext();
        var existingUser = new User
        {
            UserId = Guid.NewGuid(),
            Name = "Demo User",
            Email = "demo@example.com",
            Password = "hashed-password"
        };
        var staleCategory = new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = "Real Category",
            Type = "Expense"
        };
        var staleAccount = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Real Account",
            AccountType = "Bank Account",
            Currency = "USD",
            Vendor = "",
            MortgageType = ""
        };
        context.Users.Add(existingUser);
        context.Categories.Add(staleCategory);
        context.Accounts.Add(staleAccount);
        context.Transactions.Add(new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Account = staleAccount,
            AccountId = staleAccount.AccountId,
            Category = staleCategory,
            CategoryId = staleCategory.CategoryId,
            Amount = -123.45m,
            AmountInBaseCurrency = -123.45m,
            TransactionDate = DateTime.Today,
            TransactionType = Buenaventura.Shared.TransactionType.REGULAR
        });
        await context.SaveChangesAsync();

        var service = new AdminService(context);

        await service.ResetDemoDatabase();

        var users = await context.Users.ToListAsync();
        users.Should().ContainSingle(u => u.UserId == existingUser.UserId);
        users[0].Email.Should().Be("demo@example.com");

        (await context.Accounts.CountAsync()).Should().Be(7);
        (await context.Categories.CountAsync()).Should().BeGreaterThan(15);
        (await context.Transactions.CountAsync()).Should().BeGreaterThan(250);
        (await context.Investments.CountAsync()).Should().Be(3);
        (await context.InvestmentTransactions.CountAsync()).Should().Be(72);
        (await context.Invoices.CountAsync()).Should().Be(6);

        (await context.Categories.AnyAsync(c => c.Name == "Real Category")).Should().BeFalse();
        (await context.Accounts.AnyAsync(a => a.Name == "Real Account")).Should().BeFalse();
        (await context.Currencies.AnyAsync(c => c.Symbol == "CAD")).Should().BeTrue();
    }
}
