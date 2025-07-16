using Buenaventura.Data;
using Buenaventura.Domain;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Tests.Helpers;

public static class TestDatabaseSeeder
{
    public static async Task SeedRequiredData(BuenaventuraDbContext context)
    {
        // Clear existing data
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // Add required Currency data for CAD exchange rates
        var currencies = new List<Currency>
        {
            new Currency { CurrencyId = Guid.NewGuid(), Symbol = "USD", PriceInUsd = 1.0m, LastRetrieved = DateTime.UtcNow },
            new Currency { CurrencyId = Guid.NewGuid(), Symbol = "CAD", PriceInUsd = 0.77m, LastRetrieved = DateTime.UtcNow },
            new Currency { CurrencyId = Guid.NewGuid(), Symbol = "EUR", PriceInUsd = 1.09m, LastRetrieved = DateTime.UtcNow }
        };
        
        context.Currencies.AddRange(currencies);
        
        // Add some basic vendors
        var vendors = new List<Vendor>
        {
            new Vendor { VendorId = Guid.NewGuid(), Name = "Test Vendor 1" },
            new Vendor { VendorId = Guid.NewGuid(), Name = "Test Vendor 2" }
        };
        
        context.Vendors.AddRange(vendors);
        
        await context.SaveChangesAsync();
    }
    
    public static async Task SeedTestData(BuenaventuraDbContext context)
    {
        await SeedRequiredData(context);
        
        // Add test categories
        var categories = TestDataFactory.CategoryFaker.Generate(5);
        context.Categories.AddRange(categories);
        
        // Add test accounts
        var accounts = TestDataFactory.AccountFaker.Generate(3);
        context.Accounts.AddRange(accounts);
        
        // Add test customers
        var customers = TestDataFactory.CustomerFaker.Generate(2);
        context.Customers.AddRange(customers);
        
        // Add test investment categories
        var investmentCategories = new List<InvestmentCategory>
        {
            new InvestmentCategory { InvestmentCategoryId = Guid.NewGuid(), Name = "Stocks" },
            new InvestmentCategory { InvestmentCategoryId = Guid.NewGuid(), Name = "Bonds" },
            new InvestmentCategory { InvestmentCategoryId = Guid.NewGuid(), Name = "ETFs" }
        };
        context.InvestmentCategories.AddRange(investmentCategories);
        
        // Add test investments
        var investments = TestDataFactory.InvestmentFaker.Generate(5);
        foreach (var investment in investments)
        {
            investment.CategoryId = investmentCategories.First().InvestmentCategoryId;
        }
        context.Investments.AddRange(investments);
        
        await context.SaveChangesAsync();
    }
}
