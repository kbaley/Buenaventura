using Buenaventura.Data;
using Buenaventura.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Buenaventura.Tests.Integration;

public class BuenaventuraWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BuenaventuraDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add test database
            services.AddDbContext<BuenaventuraDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
                options.UseSnakeCaseNamingConvention();
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<BuenaventuraDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<BuenaventuraWebApplicationFactory<TStartup>>>();

            // Ensure the database is created
            db.Database.EnsureCreated();

            try
            {
                // Seed the database with test data
                SeedDatabase(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database with test messages. Error: {Message}", ex.Message);
            }
        });
    }

    private static void SeedDatabase(BuenaventuraDbContext context)
    {
        // Seed with test data
        var accounts = TestDataFactory.AccountFaker.Generate(3);
        context.Accounts.AddRange(accounts);

        var categories = TestDataFactory.CategoryFaker.Generate(5);
        context.Categories.AddRange(categories);

        var customers = TestDataFactory.CustomerFaker.Generate(3);
        context.Customers.AddRange(customers);

        var investments = TestDataFactory.InvestmentFaker.Generate(3);
        context.Investments.AddRange(investments);

        context.SaveChanges();
    }
}
