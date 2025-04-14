using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientCustomerService(HttpClient httpClient) : ICustomerService
{
    public async Task<IEnumerable<CustomerModel>> GetCustomers()
    {
        var url = "api/customers";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<CustomerModel>>(url);
        return result ?? [];
    }
}