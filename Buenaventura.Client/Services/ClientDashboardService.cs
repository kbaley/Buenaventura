using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientDashboardService(HttpClient httpClient) : IDashboardService
{
    public async Task<IEnumerable<ReportDataPoint>> GetNetWorthData()
    {
        var url = $"api/dashboard/networth";
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

    public async Task<IEnumerable<ReportDataPoint>> GetInvestmentData()
    {
        var url = $"api/dashboard/investments";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<ReportDataPoint>>(url);
        return result ?? [];
    }

    public async Task<IEnumerable<ReportDataPoint>> GetExpenseData()
    {
        var url = $"api/dashboard/expenses";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<ReportDataPoint>>(url);
        return result ?? [];
    }

    public async Task<IEnumerable<ReportDataPoint>> GetAssetClassData()
    {
        var url = $"api/dashboard/assetclasses";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<ReportDataPoint>>(url);
        return result ?? [];
    }
}