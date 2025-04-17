namespace Buenaventura.Shared;

public class CustomerModel
{
    public string Name { get; set; } = "";
    public Guid CustomerId { get; set; }
    public string Address { get; set; } = "";
    public string Email { get; set; } = "";
    public string ContactName { get; set; } = "";
}