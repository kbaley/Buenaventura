using Buenaventura.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api.Admin;

public class ScrambleDatabase(IAdminService adminService) : Endpoint<ScrambleModel>
{
    public override void Configure()
    {
        Post("api/admin/scramble");
    }

    public override async Task HandleAsync(ScrambleModel req, CancellationToken ct)
    {
        await adminService.ScrambleDatabase(req);
        await SendNoContentAsync(ct);
    }
}