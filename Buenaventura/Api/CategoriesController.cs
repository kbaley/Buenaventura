using Buenaventura.Client.Services;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<CategoryModel>> GetCategory()
    {
        return await categoryService.GetCategories();
    }

    [HttpPut("{id}")]
    public async Task PutCategory([FromRoute] Guid id, [FromBody] CategoryModel categoryModel)
    {
        await categoryService.UpdateCategory(categoryModel);
    }

    [HttpDelete("{id}")]
    public async Task DeleteCategory([FromRoute] Guid id)
    {
        await categoryService.DeleteCategory(id);
    }
}