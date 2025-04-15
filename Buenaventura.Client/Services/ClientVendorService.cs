using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientVendorService(HttpClient httpClient) : ClientService<VendorModel>("vendors", httpClient), IVendorService
{
    public async Task<IEnumerable<VendorModel>> GetVendors()
    {
        return await GetAll();
    }
}