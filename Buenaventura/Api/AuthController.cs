using Buenaventura.Client.Models;
using Buenaventura.Client.Services;
using CryptoHelper;
using Microsoft.AspNetCore.Mvc;

namespace Buenaventura.Api;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthenticationService authenticationService)
    : ControllerBase
{
    [HttpGet]
    [Route("[action]")]
    public string GetPassword(string password) {
        return Crypto.HashPassword(password);
    }

/*
        [HttpGet]
        [Route("[action]")]
        public async Task CreateUser() {
            // Quick and dirty way to create a new user
            // Fill in the details below and navigate to /api/auth/CreateUser
            var email = "";
            var name = "";
            var password = "";

            var user = new User{
                Name = name,
                Email = email,
                UserId = Guid.NewGuid(),
                Password = Crypto.HashPassword(password)
            };

            var existingUser = _context.Users.SingleOrDefault(u => u.Email == email);
            if (existingUser != null) {
                throw new Exception("User exists");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
*/

    [HttpPost]
    [Route("[action]")]
    public async Task<ActionResult<AuthData>> Login([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            return await authenticationService.Login(model.Email, model.Password);
        }
        catch (Exception exception)
        {
            return NotFound(new { message = exception.Message });
        }

    }

}