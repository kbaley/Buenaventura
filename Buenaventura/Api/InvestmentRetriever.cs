using Buenaventura.Services;

namespace Buenaventura.Api;

public interface IInvestmentRetriever : IServerAppService
{
    string RetrieveDataFor(string symbol, double start);
    Task<string> RetrieveTodaysPricesFor(IEnumerable<string> symbols);
}

public class InvestmentRetriever(IConfiguration config) : IInvestmentRetriever
{
    public async Task<string> RetrieveTodaysPricesFor(IEnumerable<string> symbols)
    {

        using var client = new HttpClient();
        const string region = "US";
        const string lang = "en";
        var symbolList = string.Join(',', symbols);
        var requestUri = $"/get-quotes?region={region}&lang={lang}&symbols={symbolList}";
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://apidojo-yahoo-finance-v1.p.rapidapi.com/market/v2" + requestUri)
        };
        request.Headers.Add("x-rapidapi-host", "apidojo-yahoo-finance-v1.p.rapidapi.com");
        request.Headers.Add("x-rapidapi-key", config.GetValue<string>("RapidApiKey"));
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }

    public string RetrieveDataFor(string symbol, double start)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://apidojo-yahoo-finance-v1.p.rapidapi.com/stock/v2");
        var frequency = "1d";
        var end = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
        var request = $"/get-historical-data?frequency={frequency}&filter=history&period1={start}&period2={end}&symbol={symbol}";
        var stringResult = File.ReadAllText(@"moo.json");
        return stringResult;
    }
}