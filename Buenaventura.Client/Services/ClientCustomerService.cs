using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientCustomerService(HttpClient httpClient) : ClientService<CustomerModel>("customers", httpClient), ICustomerService
{
    public async Task<IEnumerable<CustomerModel>> GetCustomers()
    {
        return await GetAll();
    }
}