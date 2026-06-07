namespace Buenaventura.Client.Services;

public interface IBrowserStorageService
{
    ValueTask<T?> GetItemAsync<T>(string key);
    ValueTask SetItemAsync<T>(string key, T value);
    ValueTask RemoveItemAsync(string key);
}
