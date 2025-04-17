using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface ICustomerService : IAppService
{
    Task<IEnumerable<CustomerModel>> GetCustomers(); 
    Task<CustomerModel> GetCustomer(Guid customerId);
    Task DeleteCustomer(Guid customerId);
    Task UpdateCustomer(CustomerModel customerModel);
    Task AddCustomer(CustomerModel customerModel);
}