using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Configuration;

namespace Buenaventura.Mobile.Services;

public sealed class ApiConfiguration(IConfiguration configuration)
{
    private const string BaseAddressKey = "api.base_address";

    public string BaseAddress
    {
        get => Preferences.Default.Get(BaseAddressKey, GetDefaultBaseAddress());
        set => Preferences.Default.Set(BaseAddressKey, Normalize(value));
    }

    public string GetDefaultBaseAddress()
    {
#if DEBUG
        if (DeviceInfo.Current.Platform == DevicePlatform.iOS &&
            DeviceInfo.Current.DeviceType == DeviceType.Virtual)
        {
            return GetRequiredSetting("Api:DevelopmentSimulatorBaseAddress");
        }

        return GetRequiredSetting("Api:DevelopmentDeviceBaseAddress");
#else
        return GetRequiredSetting("Api:ProductionBaseAddress");
#endif
    }

    public static string Normalize(string value)
    {
        var trimmed = value.Trim();
        if (!trimmed.EndsWith('/'))
        {
            trimmed += "/";
        }

        return trimmed;
    }

    private string GetRequiredSetting(string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing configuration value for '{key}'.");
        }

        return Normalize(value);
    }
}
