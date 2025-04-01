using Buenaventura.Client.Pages;
using Buenaventura.Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class WeatherController(IWeatherService weatherService) : ControllerBase
{

    [HttpGet]
    public async Task<IEnumerable<Weather.WeatherForecast>> GetWeatherForecast()
    {
        return await weatherService.GetForecast();
    }
}