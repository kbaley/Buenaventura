using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientInvestmentService(HttpClient httpClient) : ClientService<InvestmentForListDto>("investments", httpClient), IInvestmentService
{
    public async Task<InvestmentListModel> GetInvestments()
    {
        return await GetItem<InvestmentListModel>("");
    }

    public async Task<InvestmentListModel> UpdateCurrentPrices()
    {
        return await PostItemWithReturn<InvestmentListModel>("updatecurrentprices", null);
    }

    public async Task MakeCorrectingEntry()
    {
        await PostItem<InvestmentListModel>("makecorrectingentry", null);
    }

    public async Task DeleteInvestment(Guid investmentId)
    {
        await Delete(investmentId);
    }
}