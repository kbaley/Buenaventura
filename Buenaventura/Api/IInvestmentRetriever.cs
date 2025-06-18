using Buenaventura.Services;

namespace Buenaventura.Api;

public interface IInvestmentRetriever : IServerAppService
{
    string RetrieveDataFor(string symbol, double start);
    Task<string> RetrieveTodaysPricesFor(IEnumerable<string> symbols);
}