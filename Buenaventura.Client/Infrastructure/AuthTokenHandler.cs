using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace Buenaventura.Client.Infrastructure;

public class AuthTokenHandler(
    ILocalStorageService localStorage
) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await localStorage.GetItemAsync<string>("authToken", cancellationToken);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);

    }
}