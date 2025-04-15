using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientInvestmentService(HttpClient httpClient) : ClientService<InvestmentForListDto>("investments", httpClient), IInvestmentService
{
    public async Task<InvestmentListModel> GetInvestments()
    {
        var url = $"api/{Endpoint}";
        var result = await Client.GetFromJsonAsync<InvestmentListModel>(url);
        return result ?? new InvestmentListModel();
    }

    public async Task<InvestmentListModel> UpdateCurrentPrices()
    {
        var url = $"api/{Endpoint}/updatecurrentprices";
        var result = await Client.PostAsync(url, null);
        if (!result.IsSuccessStatusCode) throw new Exception(result.ReasonPhrase);
        var investments = await result.Content.ReadFromJsonAsync<InvestmentListModel>();
        return investments ?? new InvestmentListModel();
    }

    public async Task MakeCorrectingEntry()
    {
        var url = $"api/{Endpoint}/makecorrectingentry";
        var result = await Client.PostAsync(url, null);
        if (!result.IsSuccessStatusCode) throw new Exception(result.ReasonPhrase);
    }
}