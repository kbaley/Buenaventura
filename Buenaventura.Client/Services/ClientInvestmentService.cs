using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientInvestmentService(HttpClient httpClient) : ClientService<InvestmentModel>("investments", httpClient), IInvestmentService
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

    public async Task AddInvestment(AddInvestmentModel investmentModel)
    {
        await PostItem("", investmentModel);
    }

    public async Task BuySell(BuySellModel buySellModel)
    {
        await PostItem("buysell", buySellModel);
    }

    public async Task RecordDividend(Guid investmentId, RecordDividendModel model)
    {
        await PostItem($"{investmentId}/dividends", model);
    }
}