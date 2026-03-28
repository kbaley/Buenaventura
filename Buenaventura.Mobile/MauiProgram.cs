using System.Reflection;
using Buenaventura.Mobile.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Buenaventura.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        ConfigureAppSettings(builder.Configuration);

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<ApiConfiguration>();
        builder.Services.AddSingleton<ApiClientContext>();
        builder.Services.AddSingleton<AuthService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void ConfigureAppSettings(IConfigurationBuilder configurationBuilder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var appSettings = assembly.GetManifestResourceStream("Buenaventura.Mobile.appsettings.json")
            ?? throw new InvalidOperationException("Unable to load appsettings.json.");
        configurationBuilder.AddJsonStream(appSettings);

#if DEBUG
        using var developmentSettings = assembly.GetManifestResourceStream("Buenaventura.Mobile.appsettings.Development.json")
            ?? throw new InvalidOperationException("Unable to load appsettings.Development.json.");
        configurationBuilder.AddJsonStream(developmentSettings);
#endif
    }
}
