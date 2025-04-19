using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IInvestmentCategoryService : IAppService
{
    Task<IEnumerable<InvestmentCategoryModel>> GetCategories();
}