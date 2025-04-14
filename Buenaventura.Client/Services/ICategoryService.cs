using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface ICategoryService : IAppService
{
    Task<IEnumerable<CategoryModel>> GetCategories(); 
}

public interface ICustomerService : IAppService
{
    Task<IEnumerable<CustomerModel>> GetCustomers(); 
}