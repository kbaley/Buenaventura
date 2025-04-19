using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientInvestmentCategoryService(HttpClient httpClient) : ClientService<InvestmentCategoryModel>("investmentcategories", httpClient), IInvestmentCategoryService
{
    public async Task<IEnumerable<InvestmentCategoryModel>> GetCategories()
    {
        return await GetAll();
    }
}