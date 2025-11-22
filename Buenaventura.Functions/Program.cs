using Buenaventura;
using Buenaventura.Data;
using Buenaventura.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration.AddEnvironmentVariables();

// Add DbContext with connection string
var connectionString = builder.Configuration.GetConnectionString("Buenaventura");
builder.Services.AddDbContext<BuenaventuraDbContext>(options =>
{
    options.UseNpgsql(connectionString,
        npgOptions => { npgOptions.MigrationsHistoryTable("__ef_migrations_history", "public"); });
    options.UseSnakeCaseNamingConvention();
});

// Add Buenaventura services
builder.Services.Scan(scan => scan
    .FromAssemblyOf<AccountService>()
    .AddClasses(classes => classes.AssignableTo<IAppService>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// Add Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();