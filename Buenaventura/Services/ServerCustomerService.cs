using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerCustomerService(
    BuenaventuraDbContext context
) : ICustomerService
{
    public async Task<IEnumerable<CustomerModel>> GetCustomers()
    {
        var customers = await context.Customers
            .OrderBy(c => c.Name)
            .ToListAsync();
        return customers.Select(c => new CustomerModel
        {
            CustomerId = c.CustomerId,
            Name = c.Name,
            Address = c.Address,
            Email = c.Email,
            ContactName = c.ContactName
        });
    }

    public async Task<CustomerModel> GetCustomer(Guid customerId)
    {
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        if (customer == null)
        {
            throw new Exception("Customer not found");
        }
        return new CustomerModel
        {
            CustomerId = customer.CustomerId,
            Name = customer.Name,
            Address = customer.Address,
            Email = customer.Email,
            ContactName = customer.ContactName,
            City = customer.City,
            StreetAddress = customer.StreetAddress,
            Region = customer.Region
        };
    }

    public async Task DeleteCustomer(Guid customerId)
    {
        var customer = await context.Customers.FindAsync(customerId).ConfigureAwait(false);
        if (customer == null)
        {
            throw new Exception("Customer not found");
        }
        context.Customers.Remove(customer);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task UpdateCustomer(CustomerModel customerModel)
    {
        var customer = await context.Customers.FindAsync(customerModel.CustomerId);
        if (customer == null)
        {
            throw new Exception("Customer not found");
        }
        customer.Email = customerModel.Email;
        customer.Name = customerModel.Name;
        customer.ContactName = customerModel.ContactName;
        customer.City = customerModel.City;
        customer.StreetAddress = customerModel.StreetAddress;
        customer.Region = customerModel.Region;
        await context.SaveChangesAsync();
    }

    public async Task AddCustomer(CustomerModel customerModel)
    {
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = customerModel.Name,
            Email = customerModel.Email,
            ContactName = customerModel.ContactName,
            City = customerModel.City,
            StreetAddress = customerModel.StreetAddress,
            Region = customerModel.Region,
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }
}