namespace Buenaventura.Shared;

public class CustomerModel
{
    public string Name { get; set; } = "";
    public Guid CustomerId { get; set; }
    public string Address { get; set; } = "";
    public string Email { get; set; } = "";
    public string ContactName { get; set; } = "";
    public string StreetAddress { get; set; } = "";
    public string City { get; set; } = "";
    public string Region { get; set; } = "";
}