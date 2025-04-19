using Buenaventura.Client.Services;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Buenaventura.Api;

[Route("api/[controller]")]
[ApiController]
public class AdminController(IAdminService adminService)
    : ControllerBase
{
    [HttpPost]
    [Route("scramble")]
    public async Task ScrambleDatabase([FromBody] ScrambleModel model) {
        await adminService.ScrambleDatabase(model);
    }

}