using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IDashboardApi
{
    
    [Get("/api/dashboard/networth")]
    Task<IEnumerable<ReportDataPoint>> GetNetWorthData();
    
    [Get("/api/dashboard/creditcardbalance")]
    Task<decimal> GetCreditCardBalance();
    
    [Get("/api/dashboard/liquidassetbalance")]
    Task<decimal> GetLiquidAssetBalance();
    
    [Get("/api/dashboard/incomeexpenses")]
    Task<IEnumerable<IncomeExpenseDataPoint>> GetIncomeExpenseData();
    
    [Get("/api/dashboard/investments")]
    Task<IEnumerable<ReportDataPoint>> GetInvestmentData();
    
    [Get("/api/dashboard/assetclasses")]
    Task<IEnumerable<ReportDataPoint>> GetAssetClassData();
}