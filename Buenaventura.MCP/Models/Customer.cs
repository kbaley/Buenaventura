namespace Buenaventura.MCP.Models;
public class Customer
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; }
    public string StreetAddress { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string Email { get; set; }
    public string ContactName { get; set; }
}