using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerCustomerService(
    IDbContextFactory<CoronadoDbContext> dbContextFactory
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
        });
    }
}