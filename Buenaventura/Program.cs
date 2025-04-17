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
using Resend;

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

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(ServerAccountService));
var connectionString = builder.Configuration.GetConnectionString("Buenaventura");
builder.Services.AddDbContextFactory<BuenaventuraDbContext>(options =>
    options
        .UseSnakeCaseNamingConvention()
        .UseNpgsql(connectionString));

// Register server-side implementations of services
builder.Services
    .AddBlazoredLocalStorage()
    .AddBuenaventuraServices()
    .AddOptions()
    .AddResend(builder.Configuration)
    .AddBuenaventuraAuthentication()
    .AddScoped<IUserStore<User>, BuenaventuraUserStore>();

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