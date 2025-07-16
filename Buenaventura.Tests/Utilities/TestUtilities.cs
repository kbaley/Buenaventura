using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Buenaventura.Tests.Utilities;

public static class DatabaseSeeder
{
    public static async Task SeedTestData(BuenaventuraDbContext context)
    {
        // Clear existing data
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // Seed Accounts
        var accounts = TestDataFactory.AccountFaker.Generate(10);
        context.Accounts.AddRange(accounts);
        
        // Seed Categories
        var categories = TestDataFactory.CategoryFaker.Generate(20);
        context.Categories.AddRange(categories);
        
        // Seed Customers
        var customers = TestDataFactory.CustomerFaker.Generate(15);
        context.Customers.AddRange(customers);
        
        // Seed Investments
        var investments = TestDataFactory.InvestmentFaker.Generate(8);
        context.Investments.AddRange(investments);
        
        await context.SaveChangesAsync();
        
        // Seed Transactions (with valid foreign keys)
        var transactions = new List<Transaction>();
        foreach (var account in accounts)
        {
            var accountTransactions = TestDataFactory.TransactionFaker.Generate(50);
            accountTransactions.ForEach(t => 
            {
                t.AccountId = account.AccountId;
                t.CategoryId = categories.OrderBy(x => Guid.NewGuid()).First().CategoryId;
            });
            transactions.AddRange(accountTransactions);
        }
        
        context.Transactions.AddRange(transactions);
        
        // Seed Investment Transactions
        var investmentTransactions = new List<InvestmentTransaction>();
        foreach (var investment in investments)
        {
            var invTransactions = TestDataFactory.InvestmentTransactionFaker.Generate(10);
            invTransactions.ForEach(it => it.InvestmentId = investment.InvestmentId);
            investmentTransactions.AddRange(invTransactions);
        }
        
        context.InvestmentTransactions.AddRange(investmentTransactions);
        
        await context.SaveChangesAsync();
    }
    
    public static async Task SeedMinimalTestData(BuenaventuraDbContext context)
    {
        // Clear existing data
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // Seed minimal data for basic testing
        var account = TestDataFactory.AccountFaker.Generate();
        context.Accounts.Add(account);
        
        var category = TestDataFactory.CategoryFaker.Generate();
        context.Categories.Add(category);
        
        var customer = TestDataFactory.CustomerFaker.Generate();
        context.Customers.Add(customer);
        
        var investment = TestDataFactory.InvestmentFaker.Generate();
        context.Investments.Add(investment);
        
        await context.SaveChangesAsync();
    }
}

public static class TestHelpers
{
    public static async Task<T> WithCleanDatabase<T>(Func<BuenaventuraDbContext, Task<T>> action)
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        await context.Database.EnsureCreatedAsync();
        
        try
        {
            return await action(context);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
    
    public static async Task WithCleanDatabase(Func<BuenaventuraDbContext, Task> action)
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        await context.Database.EnsureCreatedAsync();
        
        try
        {
            await action(context);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
    
    public static async Task<T> WithSeededDatabase<T>(Func<BuenaventuraDbContext, Task<T>> action)
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        await DatabaseSeeder.SeedTestData(context);
        
        try
        {
            return await action(context);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
    
    public static async Task WithSeededDatabase(Func<BuenaventuraDbContext, Task> action)
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        await DatabaseSeeder.SeedTestData(context);
        
        try
        {
            await action(context);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
}

public static class AssertionHelpers
{
    public static void AssertTransactionEquality(Transaction expected, Transaction actual)
    {
        actual.TransactionId.Should().Be(expected.TransactionId);
        actual.AccountId.Should().Be(expected.AccountId);
        actual.Vendor.Should().Be(expected.Vendor);
        actual.Description.Should().Be(expected.Description);
        actual.Amount.Should().Be(expected.Amount);
        actual.TransactionDate.Should().Be(expected.TransactionDate);
        actual.CategoryId.Should().Be(expected.CategoryId);
        actual.TransactionType.Should().Be(expected.TransactionType);
    }
    
    public static void AssertAccountEquality(Account expected, Account actual)
    {
        actual.AccountId.Should().Be(expected.AccountId);
        actual.Name.Should().Be(expected.Name);
        actual.Currency.Should().Be(expected.Currency);
        actual.Vendor.Should().Be(expected.Vendor);
        actual.AccountType.Should().Be(expected.AccountType);
        actual.IsHidden.Should().Be(expected.IsHidden);
    }
    
    public static void AssertCategoryEquality(Category expected, Category actual)
    {
        actual.CategoryId.Should().Be(expected.CategoryId);
        actual.Name.Should().Be(expected.Name);
        actual.Type.Should().Be(expected.Type);
        actual.IncludeInReports.Should().Be(expected.IncludeInReports);
    }
    
    public static void AssertCustomerEquality(Customer expected, Customer actual)
    {
        actual.CustomerId.Should().Be(expected.CustomerId);
        actual.Name.Should().Be(expected.Name);
        actual.Email.Should().Be(expected.Email);
        actual.ContactName.Should().Be(expected.ContactName);
        actual.Address.Should().Be(expected.Address);
        actual.City.Should().Be(expected.City);
        actual.StreetAddress.Should().Be(expected.StreetAddress);
        actual.Region.Should().Be(expected.Region);
    }
    
    public static void AssertInvestmentEquality(Investment expected, Investment actual)
    {
        actual.InvestmentId.Should().Be(expected.InvestmentId);
        actual.Name.Should().Be(expected.Name);
        actual.Symbol.Should().Be(expected.Symbol);
        actual.Currency.Should().Be(expected.Currency);
        actual.DontRetrievePrices.Should().Be(expected.DontRetrievePrices);
        actual.LastPrice.Should().Be(expected.LastPrice);
        actual.CategoryId.Should().Be(expected.CategoryId);
        actual.PaysDividends.Should().Be(expected.PaysDividends);
    }
}
