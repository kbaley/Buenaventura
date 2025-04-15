using System.Net.Http.Json;

namespace Buenaventura.Client.Services;

public abstract class ClientService<T>(string endpoint, HttpClient httpClient)
{
    protected readonly HttpClient Client = httpClient;
    protected readonly string Endpoint = endpoint;

    protected async Task<IEnumerable<T>> GetAll(string? query = null)
    {
        var url = $"api/{Endpoint}";
        if (!string.IsNullOrWhiteSpace(query))
        {
            url += $"?{query}";
        }
        var result = await Client.GetFromJsonAsync<IEnumerable<T>>(url);
        return result ?? Array.Empty<T>();
    }

    protected async Task<T> Get(Guid id)
    {
        var url = $"api/{Endpoint}/{id}";
        var result = await Client.GetFromJsonAsync<T>(url);
        return result ?? throw new Exception("Item not found");
    }

    protected async Task<U> GetItem<U>(string subendpoint) where U : new()
    {
        var url = $"api/{Endpoint}/{subendpoint}";
        var result = await Client.GetFromJsonAsync<U>(url);
        return result ?? new U();
    }

    protected async Task Delete(Guid id)
    {
        var url = $"api/{Endpoint}/{id}";
        var result = await Client.DeleteAsync(url);
        if (result.IsSuccessStatusCode)
        {
            return;
        }
        throw new Exception(result.ReasonPhrase);
    }

    protected async Task Create(T item)
    {
        var url = $"api/{Endpoint}";
        var result = await Client.PostAsJsonAsync(url, item);
        if (result.IsSuccessStatusCode)
        {
            return;
        }
        throw new Exception(result.ReasonPhrase);
    }

    protected async Task Update(T item)
    {
        var url = $"api/{Endpoint}";
        var result = await Client.PutAsJsonAsync(url, item);
        if (result.IsSuccessStatusCode)
        {
            return;
        }
        throw new Exception(result.ReasonPhrase);
    }
    
    protected async Task UpdateItem<U>(string subendpoint, U item)
    {
        var url = $"api/{Endpoint}/{subendpoint}";
        var result = await Client.PutAsJsonAsync(url, item);
        if (result.IsSuccessStatusCode)
        {
            return;
        }
        throw new Exception(result.ReasonPhrase);
    }
    
    protected async Task PostItem<U>(string subendpoint, U? item)
    {
        var url = $"api/{Endpoint}/{subendpoint}";
        var result = await Client.PostAsJsonAsync(url, item);
        if (result.IsSuccessStatusCode)
        {
            return;
        }
        throw new Exception(result.ReasonPhrase);
    }
    
    protected async Task<U> PostItemWithReturn<U>(string subendpoint, U? item) where U : new()
    {
        var url = $"api/{Endpoint}/{subendpoint}";
        var result = await Client.PostAsync(url, null);
        if (!result.IsSuccessStatusCode) throw new Exception(result.ReasonPhrase);
        var returnItem = await result.Content.ReadFromJsonAsync<U>();
        return returnItem ?? new U();
    }
}