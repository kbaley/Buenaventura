using Buenaventura.Shared;
using FastEndpoints;
using ICustomerService = Buenaventura.Services.ICustomerService;

namespace Buenaventura.Api;

internal class GetCustomers(ICustomerService customerService) : EndpointWithoutRequest<IEnumerable<CustomerModel>>
{
    public override void Configure()
    {
        Get("/api/customers");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var customers = await customerService.GetCustomers();
        await SendOkAsync(customers, ct);
    }
}