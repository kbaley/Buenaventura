using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerCustomerService(
    IDbContextFactory<BuenaventuraDbContext> dbContextFactory
) : ICustomerService
{
    public async Task<IEnumerable<CustomerModel>> GetCustomers()
    {
        var context = await dbContextFactory.CreateDbContextAsync();
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

    public async Task DeleteCustomer(Guid customerId)
    {
        var context = await dbContextFactory.CreateDbContextAsync();
        var customer = await context.Customers.FindAsync(customerId).ConfigureAwait(false);
        if (customer == null)
        {
            throw new Exception("Customer not found");
        }
        context.Customers.Remove(customer);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }
}