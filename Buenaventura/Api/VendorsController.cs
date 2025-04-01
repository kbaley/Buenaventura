using Buenaventura.Data;
using Buenaventura.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class VendorsController(CoronadoDbContext context) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Vendor> GetVendors()
    {
        return context.Vendors;
    }
}
