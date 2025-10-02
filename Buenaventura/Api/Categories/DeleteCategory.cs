using FastEndpoints;
using ICategoryService = Buenaventura.Services.ICategoryService;

namespace Buenaventura.Api;

internal sealed record DeleteCategoryRequest(Guid Id);

internal class DeleteCategory(ICategoryService categoryService) : Endpoint<DeleteCategoryRequest>
{
    public override void Configure()
    {
        Delete("/api/categories/{Id}");
    }

    public override async Task HandleAsync(DeleteCategoryRequest req, CancellationToken ct)
    {
        await categoryService.DeleteCategory(req.Id);
        await SendOkAsync(ct);
    }
}
