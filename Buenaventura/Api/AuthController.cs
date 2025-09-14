using Buenaventura.Data;
using Buenaventura.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

[Route("api/[controller]")]
[ApiController]
public class AuthController(BuenaventuraDbContext context) : ControllerBase
{
    [HttpGet]
    [Route("[action]")]
    public string GetPassword(string password)
    {
        var hasher = new PasswordHasher<User>();
        var user = context.Users.First();
        var newPasswordHash = hasher.HashPassword(user, password);
        return newPasswordHash;
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<int> GetUserCount()
    {
        return await context.Users.CountAsync();
    }

    // [HttpGet]
    // [Route("[action]")]
    // public async Task CreateUser()
    // {
    //     // Quick and dirty way to create a new user
    //     // Fill in the details below and navigate to /api/auth/CreateUser
    //     var email = "stimms@gmail.com";
    //     var name = "Stimms";
    //     var password = "Password123!";

    //     var user = new User
    //     {
    //         Name = name,
    //         Email = email,
    //         UserId = Guid.NewGuid(),
    //     };

    //     var hasher = new PasswordHasher<User>();
    //     var newPasswordHash = hasher.HashPassword(user, password);
    //     user.Password = newPasswordHash;

    //     var existingUser = context.Users.SingleOrDefault(u => u.Email == email);
    //     if (existingUser != null)
    //     {
    //         throw new Exception("User exists");
    //     }

    //     context.Users.Add(user);
    //     await context.SaveChangesAsync();
    // }
}
