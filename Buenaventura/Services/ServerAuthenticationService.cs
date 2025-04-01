using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Buenaventura.Client.Models;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using CryptoHelper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Buenaventura.Services;

public class ServerAuthenticationService(CoronadoDbContext context,
    IConfiguration config) : IAuthenticationService
{
    public async Task<AuthData> Login(string username, string password)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(u => u.Email == username);

        if (user == null) {
            throw new Exception("Invalid username or password");
        }

        var validPassword = Crypto.VerifyHashedPassword(user.Password, password);
        if (!validPassword) {
            throw new Exception("Invalid password");
        }

        var jwtExpiry = config.GetValue<int>("JwtLifespan");
        var jwtSecret = config.GetValue<string>("JwtSecretKey") ?? "";
        var expiration = DateTime.UtcNow.AddSeconds(jwtExpiry);

        var tokenDescriptor = new SecurityTokenDescriptor{
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Email, user.Email)
            ]),
            Expires = expiration,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

        return new AuthData{
            Token = token,
            TokenExpirationTime = ((DateTimeOffset)expiration).ToUnixTimeSeconds(),
            Id = user.Email
        };
    }
}