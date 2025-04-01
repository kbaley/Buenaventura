using System.Text;
using Buenaventura.Client.Services;
using MudBlazor.Services;
using Buenaventura.Components;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(IAppService));
var connectionString = builder.Configuration["ConnectionStrings:Buenaventura"];
builder.Services.AddDbContext<CoronadoDbContext>(options =>
    options
        .UseSnakeCaseNamingConvention()
        .UseNpgsql(connectionString));

// Register server-side implementations of services
builder.Services.Scan(scan => scan
    .FromAssemblyOf<ServerWeatherService>()
    .AddClasses(classes => classes.InNamespaceOf<ServerWeatherService>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// Register other random app services
builder.Services.Scan(s =>
    s.FromAssemblyOf<IAppService>()
        .AddClasses(c => c.AssignableTo<IAppService>())
        .AsImplementedInterfaces()
        .WithScopedLifetime());
var jwtSecret = builder.Configuration.GetValue<string>("JwtSecretKey");
builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false
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

app.Run();