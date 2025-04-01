using System.Net.Http.Json;
using Buenaventura.Client.Pages;

namespace Buenaventura.Client.Services;

public interface IWeatherService
{
    Task<IEnumerable<Weather.WeatherForecast>> GetForecast();
}

public class ClientWeatherService(HttpClient httpClient) : IWeatherService
{
    public async Task<IEnumerable<Weather.WeatherForecast>> GetForecast()
    {
        var url = "api/weather";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<Weather.WeatherForecast>>(url);
        return result ?? [];
    }
}