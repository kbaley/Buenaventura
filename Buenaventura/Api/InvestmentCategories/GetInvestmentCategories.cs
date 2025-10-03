using Buenaventura.Client.Services;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

public class GetInvestmentCategories(IInvestmentCategoryService investmentCategoryService)
    : EndpointWithoutRequest<IEnumerable<InvestmentCategoryModel>>
{
    public override void Configure()
    {
        Get("/api/investmentcategories");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var categories = await investmentCategoryService.GetCategories();
        await SendOkAsync(categories, ct);
    }
}