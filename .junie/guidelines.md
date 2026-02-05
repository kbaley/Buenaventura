### Project Overview
- `Buenaventura` is an ASP.NET Core + Blazor solution targeting `.NET 9`.
- Server app: `Buenaventura` (FastEndpoints, EF Core, Dapper, JWT auth).
- Client app: `Buenaventura.Client` (Blazor WebAssembly, MudBlazor, Refit).
- Domain/data layer: `Buenaventura.Domain` (entities, repositories, services).
- Shared contracts: `Buenaventura.Shared` (DTOs/models used across projects).
- Azure Functions: `Buenaventura.Functions` (timer-triggered refresh job).
- Tests: `Buenaventura.Tests` (xUnit + Moq + FluentAssertions).

### Structure & Where Things Live
- UI/Pages: `Buenaventura.Client/Pages`, `Buenaventura/Components`.
- APIs/endpoints: `Buenaventura/Api` (FastEndpoints).
- Data access: `Buenaventura.Domain/Data` and migrations in `Buenaventura.Domain/Migrations`.
- Identity/auth: `Buenaventura/Identity`.
- Shared models: `Buenaventura.Shared`.

### Common Commands
Run the main web app:
```
dotnet run --project Buenaventura/Buenaventura.csproj
```

Run the tests:
```
dotnet test Buenaventura.Tests/Buenaventura.Tests.csproj
```

EF Core migrations (from repo root):
```
dotnet ef migrations add <MigrationName> --project Buenaventura.Domain/Buenaventura.Domain.csproj --startup-project Buenaventura/Buenaventura.csproj
dotnet ef database update --project Buenaventura.Domain/Buenaventura.Domain.csproj --startup-project Buenaventura/Buenaventura.csproj
```

Run Azure Functions locally:
```
dotnet run --project Buenaventura.Functions/Buenaventura.Functions.csproj
```

### Best Practices
- Keep domain logic in `Buenaventura.Domain`; UI should consume via services/clients.
- Put shared DTOs/models in `Buenaventura.Shared` to avoid duplication.
- Prefer repository/service abstractions (`Buenaventura.Domain/Data`, `Buenaventura.Domain/Services`).
- Add tests under `Buenaventura.Tests` in matching folder structure.
- Follow existing patterns for endpoints in `Buenaventura/Api` and for Razor components.