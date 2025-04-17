using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Services;

namespace Buenaventura.Api;

public interface IInvestmentPriceParser : IServerAppService
{
    Task UpdatePricesFor(BuenaventuraDbContext context);
}