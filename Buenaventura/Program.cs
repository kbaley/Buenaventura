using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Buenaventura;
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
builder.Services.AddAutoMapper(cfg => cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzg5MzQ0MDAwIiwiaWF0IjoiMTc1NzgyMTI3NSIsImFjY291bnRfaWQiOiIwMTk5NDY0ZjY3OGU3MTRhOTY2OGUxNDZjM2E1MDAwZSIsImN1c3RvbWVyX2lkIjoiY3RtXzAxazUzNTAzZjR3Z2M5eTFyNTFmZHdtZGdrIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.ivao3MRyKnTSZ-TYRB8vPO3Jkfeg5mPG5yg9G8BPTtFbpx80BIUorVz_ifoq4El_KdJKIM7CAwX3yqMsYI3oWWWUDdTuHoUKz3GtNdIuhfgptP0mSjt9yUlPtIpPOOrI80sejiju5N28kallYBTHoIQP_efVTrlW3CqpUPS6YjLJwkkKgjuCbU6TMJBQ7sySJLodn2Uz6tLUcmhLJwFG_iHmL2foA7whfPNT1gkFcnEM7b9isqEGopyu3kXyZzMFfsLFSnmSNVgadUPTJXaSmQdy-4rt1ln-5ImCJfM6tl2I8cXHGUyMvDXmAZvxcjALvlTx-JJ2XvnKfkqBq_bFCQ", typeof(ServerAccountService));
var connectionString = builder.Configuration.GetConnectionString("Buenaventura");
builder.Services.AddDbContext<BuenaventuraDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgOptions =>
    {
        npgOptions.MigrationsHistoryTable("__ef_migrations_history", "public");
    });
    options.UseSnakeCaseNamingConvention();
});

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