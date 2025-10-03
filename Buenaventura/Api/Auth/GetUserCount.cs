using Buenaventura.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

public class GetUserCount(BuenaventuraDbContext context) : EndpointWithoutRequest<int>
{
    public override void Configure()
    {
        Get("/api/auth/usercount");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var count = await context.Users.CountAsync(ct);
        await SendOkAsync(count, ct);
    }
}