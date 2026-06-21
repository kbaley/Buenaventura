using System.Security.Cryptography;
using System.Text;
using Buenaventura.Data;
using Buenaventura.Services;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

if (string.Equals(Environment.GetEnvironmentVariable("MCP_TRANSPORT"), "stdio", StringComparison.OrdinalIgnoreCase))
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
    AddBuenaventuraData(builder.Services, builder.Configuration);
    builder.Services.AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly();

    await builder.Build().RunAsync();
    return;
}

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environments.Production;
var webBuilder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = environmentName,
    ContentRootPath = Directory.GetCurrentDirectory()
});
webBuilder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddEnvironmentVariables("ASPNETCORE_");
webBuilder.Logging.AddConsole();
webBuilder.WebHost
    .UseConfiguration(webBuilder.Configuration)
    .UseKestrel();
webBuilder.Services.AddRouting();
AddBuenaventuraData(webBuilder.Services, webBuilder.Configuration);
webBuilder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = webBuilder.Build();
var allowAnonymous = app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Mcp:AllowAnonymous");
var apiKey = app.Configuration["Mcp:ApiKey"];

if (!app.Environment.IsDevelopment() && !allowAnonymous && string.IsNullOrWhiteSpace(apiKey))
{
    throw new InvalidOperationException(
        "Configure Mcp:ApiKey for bearer-token access or explicitly set Mcp:AllowAnonymous=true.");
}

app.Use(async (context, next) =>
{
    if (allowAnonymous || !context.Request.Path.StartsWithSegments("/mcp"))
    {
        await next(context);
        return;
    }

    var authorization = context.Request.Headers.Authorization.ToString();
    if (!string.IsNullOrWhiteSpace(apiKey) &&
        authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) &&
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(authorization["Bearer ".Length..]),
            Encoding.UTF8.GetBytes(apiKey)))
    {
        await next(context);
        return;
    }

    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    await context.Response.WriteAsJsonAsync(new { error = "A valid bearer token is required." });
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapMcp("/mcp");
await app.RunAsync();

static void AddBuenaventuraData(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("Buenaventura")
        ?? throw new InvalidOperationException("ConnectionStrings:Buenaventura is required.");

    services.AddDbContext<BuenaventuraDbContext>(options =>
    {
        options.UseNpgsql(connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "public"));
        options.UseSnakeCaseNamingConvention();
    });
    services.AddScoped<IFinancialQueryService, FinancialQueryService>();
}
