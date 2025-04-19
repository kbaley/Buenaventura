using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IInvestmentService : IAppService
{
    Task<InvestmentListModel> GetInvestments();
    Task<InvestmentListModel> UpdateCurrentPrices();
    
    // Makes required entry in the investments account to match the total of the portfolio
    Task MakeCorrectingEntry();
    Task DeleteInvestment(Guid investmentId);
    Task AddInvestment(AddInvestmentModel investmentModel);
}