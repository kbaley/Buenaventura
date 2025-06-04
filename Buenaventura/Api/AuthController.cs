using Buenaventura.Client.Models;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
using CryptoHelper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

[Route("api/[controller]")]
[ApiController]
public class AuthController(BuenaventuraDbContext context)
    : ControllerBase
{
    [HttpGet]
    [Route("[action]")]
    public string GetPassword(string password) {
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
            await _context.SaveChangesAsync();
        }
*/

}