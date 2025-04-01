using Buenaventura.Client.Services;
using Buenaventura.Data;

namespace Buenaventura.Api;

public interface IInvestmentPriceParser : IAppService
{
    Task UpdatePricesFor(CoronadoDbContext context);
}