using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface ICategoriesApi
{
    [Get("/api/categories")]
    Task<IEnumerable<CategoryModel>> GetCategories();
    
    [Get("/api/categories/{id}")]
    Task<CategoryModel> GetCategory(Guid id);
    
    [Put("/api/categories")]
    Task UpdateCategory(CategoryModel categoryModel);
    
    [Delete("/api/categories/{id}")]
    Task DeleteCategory(Guid id);
}
