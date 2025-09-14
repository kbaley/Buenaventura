namespace Buenaventura.MCP.Tools;

using System.ComponentModel;
using Buenaventura.MCP.Models;
using Buenaventura.MCP.Services;
using Dapper;
using ModelContextProtocol.Server;
using Npgsql;

[McpServerToolType]
public static class CustomersTool
{
    [
        McpServerTool(Name = "GetCustomers"),
        Description(
            "Get a list of customers and their ids which are used for querying details of the customers"
        )
    ]
    public static IEnumerable<Customer> GetCustomers(CustomerService customerService)
    {
        var customers = customerService.GetCustomers();
        return customers.Select(x => new Customer
        {
            CustomerId = x.CustomerId,
            Name = x.Name,
            StreetAddress = x.StreetAddress,
            City = x.City,
            Region = x.Region,
            Email = x.Email,
            ContactName = x.ContactName,
        });
    }

    [
        McpServerTool(Name = "GetCustomerDetails"),
        Description("Get details for a specific customer by id")
    ]
    public static Customer GetCustomerInfo(Guid customerId, CustomerService customerService)
    {
        var customer = customerService.GetCustomers().Where(x => x.CustomerId == customerId);
        return customer.First();
    }

    [McpServerTool(Name = "AddCustomer"), Description("Add a new customer to the database")]
    public static Guid AddCustomer(
        string name,
        string streetAddress,
        string city,
        string region,
        string email,
        string contactName,
        CustomerService customerService
    )
    {
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = name,
            StreetAddress = streetAddress,
            City = city,
            Region = region,
            Email = email,
            ContactName = contactName,
        };
        customerService.AddCustomer(customer);
        return customer.CustomerId;
    }
}
