using System.Net.Http.Json;
using Blazored.LocalStorage;
using Buenaventura.Client.Models;

namespace Buenaventura.Client.Services;

public interface IAuthenticationService
{
    Task<AuthData> Login(string username, string password);   
}

public class ClientAuthenticationService(
    ILocalStorageService localStorage,
    JwtAuthenticationStateProvider jwtAuthenticationStateProvider,
    HttpClient httpClient) : IAuthenticationService
{
    public async Task<AuthData> Login(string email, string password)
    {
        // Submit the username and password to the login API
        // If successful, store the token in local storage
        
        // If unsuccessful, throw an exception
        var model = new LoginViewModel
        {
            Email = email,
            Password = password,
        };
        var response = await httpClient.PostAsJsonAsync("api/auth/login", model);
        if (!response.IsSuccessStatusCode) return new AuthData();
        var authData = await response.Content.ReadFromJsonAsync<AuthData>();
        if (authData == null) return new AuthData();
        await localStorage.SetItemAsync("authToken", authData.Token);
        jwtAuthenticationStateProvider.MarkUserAsAuthenticated(authData.Token);
        return authData;

    }
}