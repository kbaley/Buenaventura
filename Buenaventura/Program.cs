using System.Text.Json.Serialization;
using ApexCharts;
using Blazored.LocalStorage;
using Buenaventura;
using Buenaventura.Client.Services;
using MudBlazor.Services;
using Buenaventura.Components;
using Buenaventura.Components.Account;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Identity;
using Buenaventura.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
    });
builder.Services.AddAutoMapper(typeof(ServerAccountService));
var connectionString = builder.Configuration.GetConnectionString("Buenaventura");
builder.Services.AddDbContextFactory<BuenaventuraDbContext>(options =>
    options
        .UseSnakeCaseNamingConvention()
        .UseNpgsql(connectionString, npgOptions =>
        {
            npgOptions.MigrationsHistoryTable("__ef_migrations_history", "public");
        })
    );

// Register server-side implementations of services
builder.Services
    .AddBlazoredLocalStorage()
    .AddBuenaventuraServices()
    .AddOptions()
    .AddResend(builder.Configuration)
    .AddBuenaventuraAuthentication()
    .AddScoped<IUserStore<User>, BuenaventuraUserStore>();

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
            Toolbar = new Toolbar
            {
                Show = true,
                Tools = new Tools
                {
                    Download = false,
                    Pan = false,
                    Zoomin = true,
                    Zoomout = true,
                    Zoom = true,
                    Reset = true,
                    Selection = true
                }
            }
        },
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Buenaventura.Client._Imports).Assembly);

app.MapAdditionalIdentityEndpoints();

app.Run();