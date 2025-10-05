using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Tests.Helpers;

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
                t.CategoryId = categories.OrderBy(_ => Guid.NewGuid()).First().CategoryId;
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