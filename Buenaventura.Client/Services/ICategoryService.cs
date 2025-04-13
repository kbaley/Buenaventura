using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface ICategoryService : IAppService
{
    Task<IEnumerable<CategoryDto>> GetCategories(); 
}