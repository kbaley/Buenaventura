using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace Buenaventura.Mobile.Services;

public sealed class ApiConfiguration
{
    private const string BaseAddressKey = "api.base_address";
    private const string SimulatorUrl = "https://localhost:7254/";
    private const string DevicePlaceholderUrl = "https://your-mac-hostname-or-ip:7254/";

    public string BaseAddress
    {
        get => Preferences.Default.Get(BaseAddressKey, GetDefaultBaseAddress());
        set => Preferences.Default.Set(BaseAddressKey, Normalize(value));
    }

    public static string GetDefaultBaseAddress()
    {
        if (DeviceInfo.Current.Platform == DevicePlatform.iOS &&
            DeviceInfo.Current.DeviceType == DeviceType.Virtual)
        {
            return SimulatorUrl;
        }

        return DevicePlaceholderUrl;
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
}
