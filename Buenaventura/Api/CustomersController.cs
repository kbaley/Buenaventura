using Buenaventura.Data;
using Buenaventura.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CustomersController(CoronadoDbContext context) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Customer> GetCustomer()
    {
        return context.Customers;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutCustomer([FromRoute] Guid id, [FromBody] Customer customer)
    {
        context.Entry(customer).State = EntityState.Modified;
        await context.SaveChangesAsync();

        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> PostCustomer([FromBody] Customer customer)
    {
        if (customer.CustomerId == Guid.Empty) customer.CustomerId = Guid.NewGuid();
        context.Customers.Add(customer);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return CreatedAtAction("PostCustomer", new { id = customer.CustomerId }, customer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer([FromRoute] Guid id)
    {
        var customer = await context.Customers.FindAsync(id).ConfigureAwait(false);
        if (customer == null) {
            return NotFound();
        }
        context.Customers.Remove(customer);
        await context.SaveChangesAsync().ConfigureAwait(false);
        return Ok(customer);
    }
}