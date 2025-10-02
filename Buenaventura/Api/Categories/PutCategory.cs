using Buenaventura.Shared;
using FastEndpoints;
using ICategoryService = Buenaventura.Services.ICategoryService;

namespace Buenaventura.Api;

public class PutCategory(ICategoryService categoryService) : Endpoint<CategoryModel>
{
    public override void Configure()
    {
        Put("/api/categories");
    }

    public override async Task HandleAsync(CategoryModel req, CancellationToken ct)
    {
        await categoryService.UpdateCategory(req);
        await SendOkAsync(ct);
    }
}
