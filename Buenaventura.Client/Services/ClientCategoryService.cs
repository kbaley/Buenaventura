using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientCategoryService(HttpClient httpClient) : ClientService<CategoryModel>("categories", httpClient), ICategoryService
{
    public async Task<IEnumerable<CategoryModel>> GetCategories()
    {
        return await GetAll();
    }
}