using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IDashboardService : IAppService
{
    Task<IEnumerable<ReportDataPoint>> GetNetWorthData();
    Task<decimal> GetCreditCardBalance();
    Task<decimal> GetLiquidAssetBalance();
    Task<IEnumerable<IncomeExpenseDataPoint>> GetIncomeExpenseData();
    Task<decimal> GetThisMonthExpenses();
    Task<IEnumerable<ReportDataPoint>> GetInvestmentData();
    Task<IEnumerable<ReportDataPoint>> GetExpenseData();
    Task<IEnumerable<ReportDataPoint>> GetAssetClassData();
    Task<IEnumerable<ExpenseAveragesDataPoint>> GetExpenseAveragesData();
}