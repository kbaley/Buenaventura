using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Buenaventura.Mobile.Services;

public sealed class ApiClientContext : IDisposable
{
    private readonly ApiConfiguration apiConfiguration;
    private HttpClientHandler httpClientHandler;

    public ApiClientContext(ApiConfiguration apiConfiguration)
    {
        this.apiConfiguration = apiConfiguration;
        httpClientHandler = CreateHandler();
        HttpClient = CreateClient(httpClientHandler);
    }

    public HttpClient HttpClient { get; private set; }

    public void Reset()
    {
        HttpClient.Dispose();
        httpClientHandler.Dispose();

        httpClientHandler = CreateHandler();
        HttpClient = CreateClient(httpClientHandler);
    }

    public void Dispose()
    {
        HttpClient.Dispose();
        httpClientHandler.Dispose();
    }

    private static HttpClientHandler CreateHandler() =>
        new()
        {
            CookieContainer = new CookieContainer(),
            UseCookies = true,
#if DEBUG
            ServerCertificateCustomValidationCallback = AllowDevelopmentCertificates
#endif
        };

#if DEBUG
    private static bool AllowDevelopmentCertificates(
        HttpRequestMessage requestMessage,
        X509Certificate2? _,
        X509Chain? __,
        SslPolicyErrors ___)
    {
        var host = requestMessage.RequestUri?.Host;
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (IPAddress.TryParse(host, out var address))
        {
            return IPAddress.IsLoopback(address) || IsPrivateNetwork(address);
        }

        return host.EndsWith(".local", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPrivateNetwork(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        if (bytes.Length != 4)
        {
            return false;
        }

        return bytes[0] switch
        {
            10 => true,
            172 when bytes[1] is >= 16 and <= 31 => true,
            192 when bytes[1] == 168 => true,
            _ => false
        };
    }
#endif

    private HttpClient CreateClient(HttpClientHandler handler) =>
        new(handler, disposeHandler: false)
        {
            BaseAddress = new Uri(apiConfiguration.BaseAddress)
        };
}
