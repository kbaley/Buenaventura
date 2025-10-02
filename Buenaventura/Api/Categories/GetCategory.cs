using Buenaventura.Shared;
using FastEndpoints;
using ICategoryService = Buenaventura.Services.ICategoryService;

namespace Buenaventura.Api;

internal sealed record GetCategoryRequest(Guid Id);

internal class GetCategory(ICategoryService categoryService) : Endpoint<GetCategoryRequest, CategoryModel>
{
    public override void Configure()
    {
        Get("/api/categories/{Id}");
    }

    public override async Task HandleAsync(GetCategoryRequest req, CancellationToken ct)
    {
        var category = await categoryService.GetCategory(req.Id);
        await SendAsync(category, cancellation: ct);
    }
}
