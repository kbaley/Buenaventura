using ApexCharts;
using Blazored.LocalStorage;
using Buenaventura.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
});
builder.Services.AddBlazoredLocalStorage();
// Register client-side implementations of services
builder.Services.AddSingleton(_ => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});
builder.Services.AddApexCharts(options =>
{
    options.GlobalOptions = new ApexChartBaseOptions
    {
        Theme = new Theme
        {
            Palette = PaletteType.Palette9
        },
        Chart = new Chart
        {
            Background = "transparent",
            Zoom = new Zoom
            {
                Enabled = false,
            },
            Toolbar = new Toolbar
            {
                Show = false
            }
        },
    };
});

foreach (var service in builder.Services)
{
    Console.WriteLine(service.ServiceKey);
}
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
