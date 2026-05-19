# AI Guidance

## Project Overview

Buenaventura is a multi-project .NET 10 solution for a personal finance app. The main web app is ASP.NET Core with Blazor, FastEndpoints, EF Core, PostgreSQL, MudBlazor, and Refit. There are also shared contracts, a domain/data layer, tests, a MAUI hybrid app, reusable components, and an Azure Functions timer job.

## Project Map

- `Buenaventura/`: main ASP.NET Core host, Identity setup, FastEndpoints APIs, server-side Blazor shell, app configuration.
- `Buenaventura.Client/`: Blazor WebAssembly UI, MudBlazor pages/components, Refit API clients, client services.
- `Buenaventura.Domain/`: entities, EF Core `BuenaventuraDbContext`, repositories, mappers, domain services, migrations.
- `Buenaventura.Shared/`: DTOs, view models, request/response models, enums, and other contracts shared by server/client/tests.
- `Buenaventura.Components/`: shared Razor components and component library test page.
- `Buenaventura.Tests/`: xUnit tests using FluentAssertions, Moq, Bogus, EF Core InMemory/SQLite, and `WebApplicationFactory`.
- `Buenaventura.Mobile/`: .NET MAUI Blazor Hybrid app.
- `Buenaventura.Functions/`: Azure Functions isolated worker project, including the weekday investment price refresh timer.

## Common Commands

Run from the repository root unless noted.

```bash
dotnet build Buenaventura.sln
dotnet test Buenaventura.Tests/Buenaventura.Tests.csproj
dotnet run --project Buenaventura/Buenaventura.csproj
dotnet run --project Buenaventura.Functions/Buenaventura.Functions.csproj
```

EF Core migrations:

```bash
dotnet ef migrations add <MigrationName> --project Buenaventura.Domain/Buenaventura.Domain.csproj --startup-project Buenaventura/Buenaventura.csproj
dotnet ef database update --project Buenaventura.Domain/Buenaventura.Domain.csproj --startup-project Buenaventura/Buenaventura.csproj
```

## Architecture Conventions

- Keep business rules and persistence-oriented logic in `Buenaventura.Domain`, usually behind interfaces that implement `IAppService`.
- Server services are auto-registered by `Infrastructure.AddBuenaventuraServices()` via Scrutor for classes assignable to `IAppService`.
- Put API endpoint classes under `Buenaventura/Api/<Area>/` and follow the existing FastEndpoints pattern: request type, injected service, `Configure()`, `HandleAsync()`.
- Put cross-boundary models in `Buenaventura.Shared`; avoid duplicating server/client DTO shapes.
- Client pages should call Refit API interfaces from `Buenaventura.Client/Services` rather than reaching into domain/server classes directly.
- Use MudBlazor components and the existing layout/navigation patterns for UI work.
- Add or update tests in `Buenaventura.Tests` near the matching domain/service/data area when changing behavior.

## Data And Migrations

- The primary database is PostgreSQL through EF Core and Npgsql.
- EF uses snake_case naming conventions and stores migration history in `public.__ef_migrations_history`.
- Migrations live in `Buenaventura.Domain/Migrations`.
- `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` is set in the web host; be careful when changing date/time behavior.

## Testing Notes

- Prefer focused service/domain tests for business logic.
- Use the existing helpers in `Buenaventura.Tests/Helpers` and `Buenaventura.Tests/Utilities` before adding new test infrastructure.
- Existing tests commonly use `FluentAssertions` for assertions and `Moq` for collaborator verification.

## Deployment Notes

- Deployment is handled by the GitHub Actions workflow referenced in the solution.
- Azure App Service should use startup command `dotnet Buenaventura.dll`.
- The `RefreshPricesTimer` Azure Function runs weekdays at `14:45 UTC`; the UTC schedule is intentional for Linux consumption plans.

## Keep In Mind

- Central package versions live in `Directory.Packages.props`; add or update package versions there.
- This repo has nullable reference types and implicit usings enabled across the main projects.
- Avoid broad refactors when making feature or bug-fix changes. Match the nearby style first.
- Do not treat `.junie/guidelines.md` as fully current without checking project files; it may lag behind the actual target framework.
