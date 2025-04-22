using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IDashboardService : IAppService
{
    Task<IEnumerable<ReportDataPoint>> GetNetWorthData(int? year = null);
    Task<decimal> GetCreditCardBalance();
    Task<decimal> GetLiquidAssetBalance();
}