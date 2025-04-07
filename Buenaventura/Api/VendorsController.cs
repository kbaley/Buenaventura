using Buenaventura.Client.Services;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class VendorsController(IVendorService vendorService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<VendorDto>> GetVendors()
    {
        return await vendorService.GetVendors();
    }
}
