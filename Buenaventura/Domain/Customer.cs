using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("customers")]
public class Customer
{
    [Key] public Guid CustomerId { get; set; }

    [Column(TypeName="varchar(100)")]
    [Required] public string Name { get; set; } = "";
    
    /// <summary>
    /// Used for the invoice email in the greeting
    /// </summary>
    [Column(TypeName="varchar(100)")]
    public string ContactName { get; set; } = "";
    [Column(TypeName="varchar(100)")]
    public string StreetAddress { get; set; } = "";
    [Column(TypeName="varchar(50)")]
    public string City { get; set; } = "";
    [Column(TypeName="varchar(50)")]
    public string Region { get; set; } = "";
    [Column(TypeName="varchar(50)")]
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