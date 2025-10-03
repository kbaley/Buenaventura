using Buenaventura.Shared;
using FastEndpoints;
using ICustomerService = Buenaventura.Services.ICustomerService;

namespace Buenaventura.Api;

internal class PostCustomer(ICustomerService customerService) : Endpoint<CustomerModel>
{
    public override void Configure()
    {
        Post("/api/customers");
    }

    public override async Task HandleAsync(CustomerModel req, CancellationToken ct)
    {
        await customerService.AddCustomer(req);
        await SendOkAsync(ct);
    }
}