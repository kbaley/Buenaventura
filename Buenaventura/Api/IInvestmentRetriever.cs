using Buenaventura.Client.Services;

namespace Buenaventura.Api;

public interface IInvestmentRetriever : IAppService
{
    string RetrieveDataFor(string symbol, double start);
    Task<string> RetrieveTodaysPricesFor(IEnumerable<string> symbols);
}