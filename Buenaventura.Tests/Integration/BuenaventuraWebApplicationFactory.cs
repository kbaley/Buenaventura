using Buenaventura.Data;
using Buenaventura.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Buenaventura.Tests.Integration;

public class BuenaventuraWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove any existing DbContext configurations more thoroughly
            var descriptors = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<BuenaventuraDbContext>) ||
                d.ServiceType == typeof(BuenaventuraDbContext) ||
                d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ImplementationType?.Name.Contains("PostgreSQL") == true ||
                d.ImplementationType?.Name.Contains("Npgsql") == true)
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Also remove any Entity Framework services that might conflict
            var efDescriptors = services.Where(d => 
                d.ServiceType.Namespace?.Contains("EntityFramework") == true ||
                d.ServiceType.Namespace?.Contains("Npgsql") == true)
                .ToList();

            foreach (var descriptor in efDescriptors)
            {
                services.Remove(descriptor);
            }

            // Add test database with a fresh configuration
            services.AddDbContext<BuenaventuraDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
                options.UseSnakeCaseNamingConvention();
            }, ServiceLifetime.Scoped);

            // Disable authentication for integration tests
            services.PostConfigure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
            {
                options.Filters.Clear();
            });
            
            // Replace authentication with test authentication
            services.AddAuthentication("Test")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            
            // Override authorization to allow anonymous access
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Test")
                    .RequireAssertion(_ => true)
                    .Build();
            });

            // Build the service provider to seed data
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<BuenaventuraDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<BuenaventuraWebApplicationFactory<TStartup>>>();

            try
            {
                // Seed the database with required data
                TestDatabaseSeeder.SeedTestData(db).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database with test messages. Error: {Message}", ex.Message);
                throw; // Re-throw to ensure the test fails if seeding fails
            }
        });
    }
}
