using FastEndpoints;
using ICustomerService = Buenaventura.Services.ICustomerService;

namespace Buenaventura.Api;

internal class DeleteCustomer(ICustomerService customerService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/customers/{id:guid}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        await customerService.DeleteCustomer(id);
        await SendOkAsync(ct);
    }
}