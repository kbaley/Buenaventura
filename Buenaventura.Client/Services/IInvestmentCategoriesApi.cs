using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IInvestmentCategoriesApi
{
    [Get("/api/investmentcategories")]
    public Task<IEnumerable<InvestmentCategoryModel>> GetCategories();
}