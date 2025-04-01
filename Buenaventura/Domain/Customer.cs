using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("customers")]
public class Customer
{
    [Key] public Guid CustomerId { get; set; }

    [Required] public string Name { get; set; } = "";
    public string StreetAddress { get; set; } = "";
    public string City { get; set; } = "";
    public string Region { get; set; } = "";
    public string Email { get; set; } = "";

    public string Address
    {
        get
        {
            var address = string.IsNullOrWhiteSpace(StreetAddress) ? "" : StreetAddress + "\n";
            var city = string.IsNullOrWhiteSpace(City) ? "" : City;
            if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(Region)) city += ", ";
            city += string.IsNullOrWhiteSpace(Region) ? "" : Region;
            return address + city;
        }
    }
}