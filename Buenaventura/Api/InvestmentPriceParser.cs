using Buenaventura.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Buenaventura.Api;

public class InvestmentPriceParser(IInvestmentRetriever investmentRetriever) : IInvestmentPriceParser
{
    async Task IInvestmentPriceParser.UpdatePricesFor(BuenaventuraDbContext context)
    {
        var investments = context.Investments.ToList();
        // Get symbols only for investments we have shares in
        var symbols = context.Investments
            .Include(i => i.Transactions)
            .Where(i => !i.DontRetrievePrices)
            .Select(i => new {
                i.Symbol,
                Shares = i.Transactions.Sum(t => t.Shares)
            })
            .Where(s => s.Shares != 0)
            .Select(s => s.Symbol).ToList();
        var quoteData = await investmentRetriever.RetrieveTodaysPricesFor(symbols).ConfigureAwait(false);
        var resultJson = JObject.Parse(quoteData).SelectToken("quoteResponse.result") ?? "";
        var results = JsonConvert.DeserializeObject<List<MarketPrice>>(resultJson.ToString());
        foreach (var item in results!)
        {
            var investment = investments.Where(i => i.Symbol == item.symbol);
            foreach (var i in investment)
            {
                i.LastPriceRetrievalDate = DateTime.Today;
                i.LastPrice = item.regularMarketPrice;
                context.Investments.Update(i);
            }
        }
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

}

public class MarketPrice {
    public decimal regularMarketPrice { get; set; }
    public string symbol { get; set; } = "";
}