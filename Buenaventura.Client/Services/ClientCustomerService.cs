using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientCustomerService(HttpClient httpClient) : ClientService<CustomerModel>("customers", httpClient), ICustomerService
{
    public async Task<IEnumerable<CustomerModel>> GetCustomers()
    {
        return await GetAll();
    }

    public async Task<CustomerModel> GetCustomer(Guid customerId)
    {
        return await Get(customerId);
    }

    public async Task DeleteCustomer(Guid customerId)
    {
        await Delete(customerId);
    }

    public async Task UpdateCustomer(CustomerModel customerModel)
    {
        await Put(customerModel.CustomerId, customerModel);
    }

    public async Task AddCustomer(CustomerModel customerModel)
    {
        await Post(customerModel);
    }
}