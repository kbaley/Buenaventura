using Buenaventura.Shared;
using FastEndpoints;
using ICategoryService = Buenaventura.Services.ICategoryService;

namespace Buenaventura.Api;

internal class GetCategories(ICategoryService categoryService) : EndpointWithoutRequest<IEnumerable<CategoryModel>>
{
    public override void Configure()
    {
        Get("/api/categories");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var categories = await categoryService.GetCategories();
        await SendAsync(categories, cancellation: ct);
    }
}
