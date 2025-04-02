using Buenaventura.Client.Services;
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
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpClient("AuthenticatedClient",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthenticatedClient"));
builder.Services.AddAuthenticationStateDeserialization();

await builder.Build().RunAsync();
