using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IInvestmentsApi
{
    [Get("/api/investments")]
    Task<InvestmentListModel> GetInvestments();
    
    [Post("/api/investments/updatecurrentprices")]
    Task<InvestmentListModel> UpdateCurrentPrices();
    
    // Makes required entry in the investments account to match the total of the portfolio
    [Post("/api/investments/makecorrectingentry")]
    Task MakeCorrectingEntry();
    [Delete("/api/investments/{investmentId}")]
    Task DeleteInvestment(Guid investmentId);
    [Post("/api/investments")]
    Task AddInvestment(AddInvestmentModel investmentModel);
    [Post("/api/investments/buysell")]
    Task BuySell(BuySellModel buySellModel);
    [Post("/api/investments/dividends")]
    Task RecordDividend(RecordDividendModel model);
}