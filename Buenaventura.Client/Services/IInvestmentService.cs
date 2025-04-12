using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IInvestmentService : IAppService
{
    Task<InvestmentListModel> GetInvestments();
    Task<InvestmentListModel> UpdateCurrentPrices();
}

public class ClientInvestmentService(HttpClient httpClient) : IInvestmentService
{
    public async Task<InvestmentListModel> GetInvestments()
    {
        var url = "api/investments";
        var result = await httpClient.GetFromJsonAsync<InvestmentListModel>(url);
        return result ?? new InvestmentListModel();
    }

    public async Task<InvestmentListModel> UpdateCurrentPrices()
    {
        var url = $"api/investments/updatecurrentprices";
        var result = await httpClient.PostAsync(url, null);
        if (!result.IsSuccessStatusCode) throw new Exception(result.ReasonPhrase);
        var investments = await result.Content.ReadFromJsonAsync<InvestmentListModel>();
        return investments ?? new InvestmentListModel();
    }
}