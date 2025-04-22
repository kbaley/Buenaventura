using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientDashboardService(HttpClient httpClient) : IDashboardService
{
    public async Task<IEnumerable<ReportDataPoint>> GetNetWorthData(int? year = null)
    {
        year ??= DateTime.Today.Year;
        var url = $"api/reports/networth?year={year}";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<ReportDataPoint>>(url);
        return result ?? [];
    }

    public async Task<decimal> GetCreditCardBalance()
    {
        var url = "api/dashboard/creditcardbalance";
        var result = await httpClient.GetFromJsonAsync<decimal>(url);
        return result;
    }

    public async Task<decimal> GetLiquidAssetBalance()
    {
        var url = "api/dashboard/liquidassetbalance";
        var result = await httpClient.GetFromJsonAsync<decimal>(url);
        return result;
    }

    public async Task<IEnumerable<IncomeExpenseDataPoint>> GetIncomeExpenseData()
    {
        var url = "api/dashboard/incomeexpenses";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<IncomeExpenseDataPoint>>(url);
        return result ?? [];
    }

    public async Task<decimal> GetThisMonthExpenses()
    {
        var url = "api/dashboard/expensesthismonth";
        var result = await httpClient.GetFromJsonAsync<decimal>(url);
        return result;
    }
}