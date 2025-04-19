using System.Net.Http.Json;
using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public class ClientAdminService(HttpClient client) : IAdminService
{
    public async Task ScrambleDatabase(ScrambleModel model)
    {
        var response = await client.PostAsJsonAsync("api/admin/scramble", model);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to scramble database");
        }
    }
}