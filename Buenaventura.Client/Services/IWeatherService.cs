using System.Net.Http.Json;
using Buenaventura.Client.Pages;

namespace Buenaventura.Client.Services;

public interface IWeatherService : IAppService
{
    Task<IEnumerable<Weather.WeatherForecast>> GetForecast();
}

public class ClientWeatherService(HttpClient httpClient) : IWeatherService
{
    public async Task<IEnumerable<Weather.WeatherForecast>> GetForecast()
    {
        if (httpClient.BaseAddress == null) return new List<Weather.WeatherForecast>();
        var url = "api/weather";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<Weather.WeatherForecast>>(url);
        return result ?? [];
    }
}