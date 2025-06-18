using Buenaventura.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Buenaventura.Api;

[Route("api/[controller]")]
[ApiController]
public class AccountTypesController : ControllerBase
{

    [HttpGet]
    public IEnumerable<string> GetAccountTypes( )
    {
        return AccountType.GetAccountTypes();
    }
}