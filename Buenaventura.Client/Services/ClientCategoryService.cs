using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientCategoryService(HttpClient httpClient) : ICategoryService
{
    public async Task<IEnumerable<CategoryDto>> GetCategories()
    {
        var url = "api/categories";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<CategoryDto>>(url);
        return result ?? [];
    }
}