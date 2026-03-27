using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Buenaventura.Mobile.Services;

public sealed class AuthService(ApiClientContext apiClientContext, ApiConfiguration apiConfiguration)
{
    private bool initialized;

    public event Action? AuthenticationStateChanged;

    public bool IsAuthenticated { get; private set; }
    public string? Email { get; private set; }

    public async Task InitializeAsync()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        await Task.CompletedTask;
    }

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        apiClientContext.Reset();
        var response = await apiClientContext.HttpClient.PostAsJsonAsync(
            "login?useCookies=true",
            new LoginRequest(email, password),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new LoginResult(false, await GetErrorMessageAsync(response, cancellationToken));
        }

        SetSession(email);
        return new LoginResult(true, null);
    }

    public async Task LogoutAsync()
    {
        apiClientContext.Reset();
        IsAuthenticated = false;
        Email = null;
        AuthenticationStateChanged?.Invoke();

        await Task.CompletedTask;
    }

    public string GetBaseAddress() => apiConfiguration.BaseAddress;

    public void UpdateBaseAddress(string baseAddress)
    {
        apiConfiguration.BaseAddress = baseAddress;
        apiClientContext.Reset();
        AuthenticationStateChanged?.Invoke();
    }

    private void SetSession(string? email)
    {
        IsAuthenticated = true;
        Email = email;
        AuthenticationStateChanged?.Invoke();
    }

    private static async Task<string> GetErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<LoginProblemResponse>(cancellationToken: cancellationToken);
            var firstError = problem?.Errors?.SelectMany(x => x.Value).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstError))
            {
                return firstError;
            }
        }
        catch
        {
            // Fall back to a generic message if the response isn't a validation payload.
        }

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Invalid email or password.",
            _ => $"Login failed with status code {(int)response.StatusCode}."
        };
    }

    private sealed record LoginRequest(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("password")] string Password);

    private sealed record LoginProblemResponse(
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("status")] int? Status,
        [property: JsonPropertyName("errors")] Dictionary<string, string[]>? Errors);
}

public sealed record LoginResult(bool Succeeded, string? ErrorMessage);
