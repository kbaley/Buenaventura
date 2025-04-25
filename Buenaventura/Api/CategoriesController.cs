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
    public async Task<IEnumerable<CategoryModel>> GetCategories()
    {
        return await categoryService.GetCategories();
    }

    [HttpGet("{id}")]
    public async Task<CategoryModel> GetCategory(Guid id)
    {
        return await categoryService.GetCategory(id);
    }

    [HttpPut("{id}")]
    public async Task PutCategory([FromRoute] Guid id, [FromBody] CategoryModel categoryModel)
    {
        if (id != categoryModel.CategoryId)
        {
            throw new Exception("CategoryId does not match");
        }
        await categoryService.UpdateCategory(categoryModel);
    }

    [HttpDelete("{id}")]
    public async Task DeleteCategory([FromRoute] Guid id)
    {
        await categoryService.DeleteCategory(id);
    }
}