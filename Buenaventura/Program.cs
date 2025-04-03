using System.Text;
using Buenaventura.Client.Services;
using MudBlazor.Services;
using Buenaventura.Components;
using Buenaventura.Components.Account;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Identity;
using Buenaventura.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
var connectionString = builder.Configuration["ConnectionStrings:Buenaventura"];
builder.Services.AddDbContext<CoronadoDbContext>(options =>
    options
        .UseSnakeCaseNamingConvention()
        .UseNpgsql(connectionString));

// Register server-side implementations of services
builder.Services.Scan(scan => scan
    .FromAssemblyOf<ServerAccountService>()
    .AddClasses(classes => classes.AssignableTo<IServerAppService>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());
builder.Services.Scan(scan => scan
    .FromAssemblyOf<ServerAccountService>()
    .AddClasses(classes => classes.AssignableTo<IAppService>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddIdentityCore<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = false;
    })
    .AddUserStore<BuenaventuraUserStore>()
    // .AddRoleStore<BuenaventuraRoleStore>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserStore<User>, BuenaventuraUserStore>();
builder.Services.AddScoped<IUserPasswordStore<User>, BuenaventuraUserStore>();

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