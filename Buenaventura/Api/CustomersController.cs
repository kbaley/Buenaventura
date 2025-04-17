using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CustomersController(BuenaventuraDbContext context, ICustomerService customerService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<CustomerModel>> GetCustomers()
    {
        return await customerService.GetCustomers();
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
        await customerService.DeleteCustomer(id);
        return Ok();
    }
}