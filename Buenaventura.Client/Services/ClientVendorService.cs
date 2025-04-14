using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientVendorService(HttpClient httpClient) : IVendorService
{
    public async Task<IEnumerable<VendorModel>> GetVendors()
    {
        var url = "api/vendors";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<VendorModel>>(url);
        return result ?? [];
    }
}