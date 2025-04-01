using Buenaventura.Client.Pages;
using Buenaventura.Client.Services;

namespace Buenaventura.Services;

public class ServerWeatherService : IWeatherService
{
    public Task<IEnumerable<Weather.WeatherForecast>> GetForecast()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        var forecasts = Enumerable.Range(1, 5).Select(index => new Weather.WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        });
        return Task.FromResult(forecasts);
    }
}