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

    [HttpGet("{id}")]
    public async Task<CustomerModel> GetCustomer([FromRoute] Guid id)
    {
        return await customerService.GetCustomer(id);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCustomer([FromRoute] Guid id, [FromBody] CustomerModel customer)
    {
        await customerService.UpdateCustomer(customer);

        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> PostCustomer([FromBody] CustomerModel customer)
    {
        await customerService.AddCustomer(customer);
        return CreatedAtAction("PostCustomer", null);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer([FromRoute] Guid id)
    {
        await customerService.DeleteCustomer(id);
        return Ok();
    }
}