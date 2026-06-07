using Buenaventura.Services;
using FastEndpoints;

namespace Buenaventura.Api.Admin;

public class ResetDemoDatabase(IAdminService adminService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("api/admin/reset-demo-database");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await adminService.ResetDemoDatabase();
        await Send.NoContentAsync(ct);
    }
}
