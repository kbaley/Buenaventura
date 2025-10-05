using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using Buenaventura.MCP.Tools;
using Buenaventura.MCP.Configuration;
using Buenaventura.MCP.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<DatabaseConfiguration>(
    builder.Configuration.GetSection("ConnectionStrings")
);

// Log the connection string at startup
var config = builder.Configuration.GetSection("ConnectionStrings").Get<Buenaventura.MCP.Configuration.DatabaseConfiguration>();
Console.WriteLine($"Startup: Connection string for Buenaventura: {config?.Buenaventura}");


builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<AccountService>();
builder.Services.AddSingleton<CustomerService>();
builder.Services.AddSingleton<TransactionService>();

builder.Services.AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly();


await builder.Build().RunAsync();

