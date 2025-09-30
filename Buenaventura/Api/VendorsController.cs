using Buenaventura.Client.Services;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IVendorService = Buenaventura.Services.IVendorService;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class VendorsController(IVendorService vendorService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<VendorModel>> GetVendors()
    {
        return await vendorService.GetVendors();
    }
}
