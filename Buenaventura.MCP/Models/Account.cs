namespace Buenaventura.MCP.Models;

public class Account
{
    public Guid AccountId { get; set; }
    public string Name { get; set; }
    public string Currency { get; set; }
    public string Vendor { get; set; }
    public string AccountType { get; set; }
}