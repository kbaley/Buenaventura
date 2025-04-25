using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientCategoryService(HttpClient httpClient) : ClientService<CategoryModel>("categories", httpClient), ICategoryService
{
    public async Task<IEnumerable<CategoryModel>> GetCategories()
    {
        return await GetAll();
    }

    public async Task DeleteCategory(Guid id)
    {
        await Delete(id);
    }

    public async Task UpdateCategory(CategoryModel categoryModel)
    {
        if (categoryModel.CategoryId == null)
        {
            throw new ArgumentNullException(nameof(categoryModel.CategoryId), "CategoryId cannot be null");
        }
        await Put(categoryModel.CategoryId.Value, categoryModel);
    }

    public async Task<CategoryModel> GetCategory(Guid id)
    {
        return await Get(id);
    }
}