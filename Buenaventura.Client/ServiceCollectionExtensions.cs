using System.Text.Json;
using Refit;

namespace Buenaventura.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRefit<TApi>(this IServiceCollection services, string baseAddress)
        where TApi : class
    {
        // Use web options so enums are serialized as integers
        var jsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Web);
        var settings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
        };
        services.AddRefitClient<TApi>(settings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));
        return services;
    }
}