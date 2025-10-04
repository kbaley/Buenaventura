using Buenaventura.Client.Services;
using Buenaventura.Domain;
using Buenaventura.Identity;
using Buenaventura.Services;
using Microsoft.AspNetCore.Identity;
using Resend;

namespace Buenaventura;

public static class Infrastructure
{
    public static IServiceCollection AddResend(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(o => { o.ApiToken = configuration["ResendApiKey"]!; });
        services.AddTransient<IResend, ResendClient>();
        return services;
    }

    public static IServiceCollection AddBuenaventuraAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();
        services.AddIdentityCore<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = false;
            })
            .AddUserStore<BuenaventuraUserStore>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        services.AddScoped<IUserPasswordStore<User>, BuenaventuraUserStore>();
        return services;
    }

    public static IServiceCollection AddBuenaventuraServices(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<AccountService>()
            .AddClasses(classes => classes.AssignableTo<IAppService>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        services.AddSingleton<AccountSyncService>();
        return services;
    }
}