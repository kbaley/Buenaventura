using Blazored.LocalStorage;
using Buenaventura.Client;
using Buenaventura.Client.Infrastructure;
using Buenaventura.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddMudServices();
// Register client-side implementations of services
builder.Services.Scan(scan => scan
    .FromAssemblyOf<ClientWeatherService>()
    .AddClasses(classes => classes.InNamespaceOf<ClientWeatherService>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());
builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddHttpClient("AuthenticatedClient",
        client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthenticatedClient"));
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
