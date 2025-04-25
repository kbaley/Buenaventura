using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface ICategoryService : IAppService
{
    Task<IEnumerable<CategoryModel>> GetCategories();
    Task DeleteCategory(Guid id);
    Task UpdateCategory(CategoryModel categoryModel);
}