using Buenaventura.Data;
using Buenaventura.Domain;
using Newtonsoft.Json;

namespace Buenaventura.Services;

public interface ICurrencyService : IServerAppService
{
    Task<decimal> GetExchangeRateFor(string symbol, CancellationToken ct = default);
}

public class CurrencyService(BuenaventuraDbContext context) : ICurrencyService
{

    public async Task<decimal> GetExchangeRateFor(string symbol, CancellationToken ct = default)
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
                var response = await client.GetAsync($"/latest?base=USD&symbols={symbol}", ct);
                response.EnsureSuccessStatusCode();
                var stringResult = await response.Content.ReadAsStringAsync(ct);
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
                await context.SaveChangesAsync(ct);
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