using System.Text.RegularExpressions;

namespace Buenaventura.Data
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }

        public static string GetAddress(string address, string city, string region)
        {
            var formattedAddress = string.IsNullOrWhiteSpace(address) ? "" : address + "\n";
            var formattedCity = string.IsNullOrWhiteSpace(city) ? "" : city;
            if (!string.IsNullOrWhiteSpace(formattedCity) && !string.IsNullOrWhiteSpace(region)) formattedCity += ", ";
            formattedCity += string.IsNullOrWhiteSpace(region) ? "" : region;
            return formattedAddress + formattedCity;
        }
    }

    public static class GuidExtensions
    {
        public static Guid GetId(this Guid id)
        {
            return id == Guid.Empty ? Guid.NewGuid() : id;
        }

        public static bool IsNullOrEmpty(this Guid? id) {
            return !id.HasValue || id.Value == Guid.Empty;
        }
    }
}