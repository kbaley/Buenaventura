using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface ICustomersApi
{
    [Get("/api/customers")]
    Task<IEnumerable<CustomerModel>> GetCustomers(); 
    
    [Get("/api/customers/{customerId}")]
    Task<CustomerModel> GetCustomer(Guid customerId);
    
    [Delete("/api/customers/{customerId}")]
    Task DeleteCustomer(Guid customerId);
    
    [Put("/api/customers")]
    Task UpdateCustomer(CustomerModel customerModel);
    
    [Post("/api/customers")]
    Task AddCustomer(CustomerModel customerModel);
}