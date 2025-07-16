using Buenaventura.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Buenaventura.Tests.Helpers;

public static class TestDbContextFactory
{
    public static BuenaventuraDbContext CreateInMemoryDbContext()
    {
        var services = new ServiceCollection();
        
        services.AddEntityFrameworkInMemoryDatabase();
        services.AddDbContext<BuenaventuraDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                   .UseSnakeCaseNamingConvention());
        
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BuenaventuraDbContext>();
        
        return context;
    }
    
    public static BuenaventuraDbContext CreateSqliteDbContext()
    {
        var services = new ServiceCollection();
        
        services.AddEntityFrameworkSqlite();
        services.AddDbContext<BuenaventuraDbContext>(options =>
            options.UseSqlite($"Data Source=:memory:")
                   .UseSnakeCaseNamingConvention());
        
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BuenaventuraDbContext>();
        
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        
        return context;
    }
}

public class TestDbContextFixture : IDisposable
{
    public BuenaventuraDbContext Context { get; private set; }
    
    public TestDbContextFixture()
    {
        Context = TestDbContextFactory.CreateInMemoryDbContext();
    }
    
    public void Dispose()
    {
        Context?.Dispose();
    }
}
