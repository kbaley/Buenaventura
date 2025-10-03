using Buenaventura.Data;
using Buenaventura.Domain;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;

namespace Buenaventura.Api;

internal record HashPasswordRequest(string Password);
internal record HashPasswordResponse(string HashedPassword);

/// <summary>
/// Get a hashed password from a plain text password
/// </summary>
internal class HashPassword(BuenaventuraDbContext context) : Endpoint<HashPasswordRequest, HashPasswordResponse>
{
    
    public override void Configure()
    {
        Post("/api/auth/hashpassword");
        AllowAnonymous();
    }

    public override async Task HandleAsync(HashPasswordRequest req, CancellationToken ct)
    {
        var hasher = new PasswordHasher<User>();
        var user = context.Users.First();
        var hashedPassword = hasher.HashPassword(user, req.Password);
        await SendAsync(new HashPasswordResponse(hashedPassword), cancellation: ct);
    }
}