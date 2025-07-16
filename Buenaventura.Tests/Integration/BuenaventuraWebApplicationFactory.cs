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
            // Remove any existing DbContext configurations
            var descriptors = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<BuenaventuraDbContext>) ||
                d.ServiceType == typeof(BuenaventuraDbContext) ||
                d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add test database
            services.AddDbContext<BuenaventuraDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                options.UseSnakeCaseNamingConvention();
            });
        });

        builder.ConfigureServices(services =>
        {
            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<BuenaventuraDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<BuenaventuraWebApplicationFactory<TStartup>>>();

            try
            {
                // Seed the database with required data
                Task.Run(async () => await TestDatabaseSeeder.SeedTestData(db)).Wait();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database with test messages. Error: {Message}", ex.Message);
            }
        });
    }
}
