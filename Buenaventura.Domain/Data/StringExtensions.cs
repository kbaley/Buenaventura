namespace Buenaventura.Data
{
    public static class StringExtensions
    {
        public static string GetAddress(string address, string city, string region)
        {
            var formattedAddress = string.IsNullOrWhiteSpace(address) ? "" : address + "\n";
            var formattedCity = string.IsNullOrWhiteSpace(city) ? "" : city;
            if (!string.IsNullOrWhiteSpace(formattedCity) && !string.IsNullOrWhiteSpace(region)) formattedCity += ", ";
            formattedCity += string.IsNullOrWhiteSpace(region) ? "" : region;
            return formattedAddress + formattedCity;
        }
    }
}