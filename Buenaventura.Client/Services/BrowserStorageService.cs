using System.Text.Json;
using Microsoft.JSInterop;

namespace Buenaventura.Client.Services;

public sealed class BrowserStorageService(IJSRuntime jsRuntime) : IBrowserStorageService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public async ValueTask<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            return string.IsNullOrWhiteSpace(json)
                ? default
                : JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
        }
        catch (InvalidOperationException)
        {
            return default;
        }
        catch (JSDisconnectedException)
        {
            return default;
        }
    }

    public async ValueTask SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonSerializerOptions);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSDisconnectedException)
        {
        }
    }

    public async ValueTask RemoveItemAsync(string key)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSDisconnectedException)
        {
        }
    }
}
