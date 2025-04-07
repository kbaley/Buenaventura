using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IVendorService : IAppService
{
    public Task<IEnumerable<VendorDto>> GetVendors();
}
public class ClientVendorService(HttpClient httpClient) : IVendorService
{
    public async Task<IEnumerable<VendorDto>> GetVendors()
    {
        var url = "api/vendors";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<VendorDto>>(url);
        return result ?? [];
    }
}
