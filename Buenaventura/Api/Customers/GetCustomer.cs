using Buenaventura.Shared;
using FastEndpoints;
using ICustomerService = Buenaventura.Services.ICustomerService;

namespace Buenaventura.Api;

internal class GetCustomer(ICustomerService customerService) : EndpointWithoutRequest<CustomerModel>
{
    public override void Configure()
    {
        Get("/api/customers/{id:guid}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var customer = await customerService.GetCustomer(id);
        await SendOkAsync(customer, ct);
    }
}