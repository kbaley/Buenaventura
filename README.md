# Buenaventura Personal Finance App

Buenaventura is a .NET 10 rewrite of [Coronado](https://github.com/kbaley/Coronado), the personal finance app I built after trying QuickBooks, YNAB, spreadsheets, Wave, Zoho, Pocketsmith, and a few others. The goal is still the same: one place for personal and small-business finances, especially when banking is manual, multi-currency, and not neatly separated into "business" and "personal" buckets.

The original Coronado README has a lot of project history. This README keeps the parts that still apply and updates the architecture for the current Blazor, FastEndpoints, EF Core, and PostgreSQL version.

## What It Tracks

- Accounts, transactions, transfers, categories, vendors, customers, and invoices.
- Dashboards for net worth, liquid assets, credit card balances, income/expense trends, asset classes, and investments.
- Expense analysis by category, vendor, tags, and time period.
- Budget planning, account reconciliation, reimbursements, bulk transaction entry, and transaction CSV export.
- Investments, investment categories, current price updates, buy/sell transactions, dividends, and correcting entries that reconcile investment holdings back to the reporting account.
- Demo/admin workflows for resetting or scrambling data.

Some Coronado-era features are not documented here because they are either gone or not the main local workflow anymore: the React client, console app, old keyboard shortcuts, manual VS Code Azure deployment, and the SQL Server branch notes.

## Architecture

Buenaventura is a multi-project solution:

- `Buenaventura/`: ASP.NET Core host. It wires up Identity, Razor components, the Blazor WebAssembly client, FastEndpoints APIs, EF Core, authentication, Resend email, and service registration.
- `Buenaventura.Client/`: Blazor WebAssembly UI using MudBlazor and ApexCharts. Pages call API interfaces through Refit instead of reaching into server or domain classes.
- `Buenaventura.Domain/`: domain entities, `BuenaventuraDbContext`, EF Core migrations, repositories, mappers, and application services.
- `Buenaventura.Shared/`: DTOs, request/response models, view models, enums, and other contracts shared by the server, client, and tests.
- `Buenaventura.Components/`: reusable Razor components and a component library test page.
- `Buenaventura.Tests/`: xUnit tests for domain, service, data, parser, integration, and performance behavior.
- `Buenaventura.Functions/`: Azure Functions isolated worker app. The timer job updates investment prices on weekdays.
- `Buenaventura.Mobile/`: .NET MAUI Blazor Hybrid app.
- `Buenaventura.MCP/`: read-only Model Context Protocol server for finance queries from Codex or ChatGPT.

The usual request flow is:

1. A Blazor page or component in `Buenaventura.Client` injects a Refit interface from `Buenaventura.Client/Services`.
2. The Refit interface calls a FastEndpoints endpoint under `Buenaventura/Api/<Area>/`.
3. The endpoint delegates business behavior to an interface implemented in `Buenaventura.Domain/Services`.
4. Domain services use the EF Core `BuenaventuraDbContext`, repositories, and shared DTOs/models as needed.

Server-side application services are registered automatically by `Infrastructure.AddBuenaventuraServices()`, which scans for classes assignable to `IAppService` and registers their implemented interfaces with scoped lifetime. If you add a new domain service, implement an interface that inherits `IAppService` and it will be picked up by the web app and function app service registration.

## Tech Stack

- .NET 10
- ASP.NET Core, Razor components, and Blazor WebAssembly
- FastEndpoints for HTTP APIs
- MudBlazor and ApexCharts for the UI
- Refit for typed client API calls
- EF Core with Npgsql and snake_case naming conventions
- PostgreSQL as the primary database
- ASP.NET Core Identity with custom Buenaventura user/role stores
- Resend for invoice email
- RapidAPI/Yahoo Finance integration for investment price retrieval
- Azure Functions isolated worker for scheduled price refresh

PostgreSQL carries over from Coronado. It started as a practical choice because the original app was built on macOS with a local PostgreSQL container, and it remains the supported database here. There is no current SQL Server migration path documented for Buenaventura.

## Working Locally

Prerequisites:

- .NET 10 SDK
- PostgreSQL
- EF Core CLI tools if you are adding or applying migrations:

```bash
dotnet tool install --global dotnet-ef
```

From the repository root:

```bash
dotnet restore Buenaventura.sln
dotnet build Buenaventura.sln
dotnet test Buenaventura.Tests/Buenaventura.Tests.csproj
```

Configure a local PostgreSQL connection string. Prefer user secrets or environment variables so local credentials do not get copied into source-controlled config:

```bash
dotnet user-secrets init --project Buenaventura/Buenaventura.csproj
dotnet user-secrets set "ConnectionStrings:Buenaventura" "Host=localhost;Port=5432;Database=buenaventura;Username=postgres;Password=<password>" --project Buenaventura/Buenaventura.csproj
```

Optional service keys can also be configured with user secrets:

```bash
dotnet user-secrets set "RapidApiKey" "<rapidapi-key>" --project Buenaventura/Buenaventura.csproj
dotnet user-secrets set "Html2PdfRocketKey" "<html2pdfrocket-key>" --project Buenaventura/Buenaventura.csproj
dotnet user-secrets set "ResendApiKey" "<resend-key>" --project Buenaventura/Buenaventura.csproj
```

Apply migrations:

```bash
dotnet ef database update --project Buenaventura.Domain/Buenaventura.Domain.csproj --startup-project Buenaventura/Buenaventura.csproj
```

Run the web app:

```bash
dotnet run --project Buenaventura/Buenaventura.csproj
```

The launch profile uses `http://localhost:5094` and `https://localhost:7254` for local development.

## Adding Features

Use the existing project boundaries:

- Put shared request/response shapes in `Buenaventura.Shared`.
- Put persistence and business behavior in `Buenaventura.Domain`, usually behind an `IAppService` interface.
- Put HTTP endpoints in `Buenaventura/Api/<Area>/` and follow the existing FastEndpoints pattern.
- Put client calls in `Buenaventura.Client/Services` as Refit interfaces.
- Put UI in `Buenaventura.Client/Pages`, `Buenaventura.Client/Components`, or `Buenaventura.Components` depending on reuse.
- Add focused tests in `Buenaventura.Tests` near the matching domain, service, data, or integration area.

## EF Migrations

To add a migration:

```bash
dotnet ef migrations add <MigrationName> --project Buenaventura.Domain/Buenaventura.Domain.csproj --startup-project Buenaventura/Buenaventura.csproj
```

To update the database:

```bash
dotnet ef database update --project Buenaventura.Domain/Buenaventura.Domain.csproj --startup-project Buenaventura/Buenaventura.csproj
```

Migrations live in `Buenaventura.Domain/Migrations`. EF uses snake_case naming conventions and stores migration history in `public.__ef_migrations_history`. The web host and function app both set `Npgsql.EnableLegacyTimestampBehavior`, so be careful when changing date/time behavior.

## Investment Price Refresh Function

`Buenaventura.Functions` contains `RefreshPricesTimer`, an Azure Functions isolated worker timer. It runs every weekday at `14:45 UTC` and calls `IInvestmentService.UpdateCurrentPrices()`, similar to refreshing prices manually on the Investments page.

The schedule intentionally uses UTC instead of `WEBSITE_TIME_ZONE` because Azure Functions does not support setting the time zone for Linux consumption plans. See Microsoft's timer trigger time zone notes for details:

https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer#ncrontab-time-zones

Run the function project locally with:

```bash
dotnet run --project Buenaventura.Functions/Buenaventura.Functions.csproj
```

For local function development, configure the same `ConnectionStrings:Buenaventura` and `RapidApiKey` values in the function app's local settings or environment.

## Deployment

Deployment is handled by `.github/workflows/azure-webapps-dotnet-core.yml`.

Azure App Service should use this startup command:

```bash
dotnet Buenaventura.dll
```

This became required after moving to .NET 10, likely because multiple runtime configuration files are present in the published output and the startup command makes the intended assembly explicit.

## MCP Server

`Buenaventura.MCP` exposes three read-only tools:

- `get_transactions`: searches income and expense transactions over a bounded date range.
- `summarize_finances`: totals income and expenses by category, including invoiced income.
- `list_financial_categories`: lists configured income and expense categories.

Set the connection string with an environment variable; do not put it in `appsettings`:

```bash
export ConnectionStrings__Buenaventura="Host=localhost;Port=5432;Database=buenaventura;Username=postgres;Password=<password>"
```

Run the Streamable HTTP server locally:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project Buenaventura.MCP/Buenaventura.MCP.csproj
```

The health endpoint is `/health` and the MCP endpoint is `/mcp`. Development permits anonymous MCP calls. Production fails to start unless either `Mcp__ApiKey` is set or `Mcp__AllowAnonymous=true` is explicitly configured.

### Use locally with Codex

Codex can start the server over stdio, so no HTTP server or Azure deployment is required. Replace the project path with the absolute path on your machine:

```bash
codex mcp add buenaventura \
  --env MCP_TRANSPORT=stdio \
  --env ConnectionStrings__Buenaventura="$ConnectionStrings__Buenaventura" \
  -- dotnet run --project /absolute/path/to/Buenaventura.MCP/Buenaventura.MCP.csproj
```

Restart Codex after adding the server. The Codex app, CLI, and IDE extension share this MCP configuration.

### Deploy to Azure App Service

Create a separate Linux App Service for the MCP project and configure:

- Startup command: `dotnet Buenaventura.MCP.dll`
- `ConnectionStrings__Buenaventura`: the PostgreSQL connection string
- `Mcp__ApiKey`: a long random token for Codex remote access

Configure the GitHub environment named `MCP` with:

- Repository/environment variable `AZURE_MCP_WEBAPP_NAME`
- Secret `AZURE_MCP_PUBLISH_PROFILE`

The `azure-mcp.yml` workflow can then deploy the project manually or after relevant changes reach `main`.

For remote Codex access, configure the Azure endpoint and keep the token in an environment variable:

```toml
[mcp_servers.buenaventura]
url = "https://<app-name>.azurewebsites.net/mcp"
bearer_token_env_var = "BUENAVENTURA_MCP_TOKEN"
```

### Connect ChatGPT developer mode

ChatGPT connects to the HTTPS `/mcp` endpoint. For this minimal first version, set `Mcp__AllowAnonymous=true` and choose **No authentication** when creating the app in ChatGPT developer mode.

This anonymous mode exposes financial data to anyone who can reach the endpoint. Use it only for a short-lived personal experiment, add Azure network restrictions where practical, and move to standards-based OAuth before treating the deployment as permanent. ChatGPT cannot use the server's custom bearer-token option. To test a local HTTP server with ChatGPT, expose it temporarily through an HTTPS tunnel and use the tunnel's `/mcp` URL.

## Monitoring

In the Azure Portal, navigate to the Function App and select `Monitoring` -> `Logs` to inspect function logs.

Use Application Insights tables such as `exceptions` for failures. This query shows recent timer activity:

```kusto
traces
| where timestamp > ago(1h)
| where message contains "RefreshPricesTimer" or customDimensions.Category contains "RefreshPricesTimer"
| order by timestamp desc
```

To trigger the function manually, use Azure Explorer in Rider: right-click the function and choose `Trigger Function with Http Client`.
