using Buenaventura.Client.Services;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TransactionsController(IAccountService accountService)
    : ControllerBase
{
    // [HttpDelete("{id}")]
    // public async Task<IActionResult> DeleteTransaction([FromRoute] Guid id)
    // {
    //     if (!ModelState.IsValid)
    //     {
    //         return BadRequest(ModelState);
    //     }
    //
    //     await accountService.DeleteTransaction(id);
    //     return Ok();
    // }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction([FromRoute] Guid id,
        [FromBody] TransactionForDisplay transaction)
    {

        if (id != transaction.TransactionId)
        {
            return BadRequest();
        }

        await accountService.UpdateTransaction(transaction);
        return Ok();
    }

}