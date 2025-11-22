using Buenaventura.Data;
// using Buenaventura.Domain;
// using CryptoHelper;
using FastEndpoints;

namespace Buenaventura.Api;

public class CreateUser(BuenaventuraDbContext context) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/api/auth/CreateUser");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Quick and dirty way to create a new user; uncomment to implement
        // Fill in the details below and navigate to /api/auth/CreateUser
        // var email = "";
        // var name = "";
        // var password = "";
        //
        // var user = new User{
        //     Name = name,
        //     Email = email,
        //     UserId = Guid.NewGuid(),
        //     Password = Crypto.HashPassword(password)
        // };
        //
        // var existingUser = context.Users.SingleOrDefault(u => u.Email == email);
        // if (existingUser != null) {
        //     throw new Exception("User exists");
        // }
        //
        // context.Users.Add(user);
        // await context.SaveChangesAsync(ct);
        await SendOkAsync(ct);
    }

}