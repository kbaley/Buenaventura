using Blazored.LocalStorage;
using Buenaventura.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
// Register client-side implementations of services
builder.Services.AddSingleton(_ => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});
builder.Services.Scan(scan => scan
    .FromAssemblyOf<ClientAccountService>()
    .AddClasses(classes => classes.AssignableTo<IAppService>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services
    .AddSingleton<AccountSyncService>();

await builder.Build().RunAsync();
