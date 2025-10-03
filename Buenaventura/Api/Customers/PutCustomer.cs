using Buenaventura.Shared;
using FastEndpoints;
using ICustomerService = Buenaventura.Services.ICustomerService;

namespace Buenaventura.Api;

internal class PutCustomer(ICustomerService customerService) : Endpoint<CustomerModel, CustomerModel>
{
    public override void Configure()
    {
        Put("/api/customers");
    }

    public override async Task HandleAsync(CustomerModel req, CancellationToken ct)
    {
        await customerService.UpdateCustomer(req);
        await SendOkAsync(req, ct);
    }
}