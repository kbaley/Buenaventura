using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IInvestmentService : IAppService
{
    Task<InvestmentListModel> GetInvestments();
}

public class ClientInvestmentService(HttpClient httpClient) : IInvestmentService
{
    public async Task<InvestmentListModel> GetInvestments()
    {
        var url = "api/investments";
        var result = await httpClient.GetFromJsonAsync<InvestmentListModel>(url);
        return result ?? new InvestmentListModel();
    }
}