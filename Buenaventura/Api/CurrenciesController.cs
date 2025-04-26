using Buenaventura.Data;
using Buenaventura.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CurrenciesController(BuenaventuraDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<decimal> GetExchangeRateFor(string symbol)
    {
        var currency = context.Currencies
            .OrderByDescending(c => c.LastRetrieved)
            .FirstOrDefault(c => c.Symbol == symbol);
        if (currency == null || currency.LastRetrieved < DateTime.Today)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.exchangerate.host");
            try
            {
                var response = await client.GetAsync($"/latest?base=USD&symbols={symbol}");
                response.EnsureSuccessStatusCode();
                var stringResult = await response.Content.ReadAsStringAsync();
                dynamic rawRate = JsonConvert.DeserializeObject(stringResult) ??
                                  new { rates = new Dictionary<string, decimal>() };
                currency = new Currency
                {
                    CurrencyId = Guid.NewGuid(),
                    Symbol = symbol,
                    PriceInUsd = rawRate.rates[symbol],
                    LastRetrieved = DateTime.Today
                };
                context.Currencies.Add(currency);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                // For now, do nothing
                Console.WriteLine(e.Message);
            }
        }

        return currency!.PriceInUsd;
    }
}