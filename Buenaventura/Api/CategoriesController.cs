using Buenaventura.Client.Services;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ICategoryService = Buenaventura.Services.ICategoryService;

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

    [HttpGet("{id}/expenses")]
    public async Task<CategoryModel> GetCategoryExpenses(Guid id)
    {
        return await categoryService.GetCategory(id);
    }

    [HttpPut]
    public async Task PutCategory([FromBody] CategoryModel categoryModel)
    {
        await categoryService.UpdateCategory(categoryModel);
    }

    [HttpDelete("{id}")]
    public async Task DeleteCategory([FromRoute] Guid id)
    {
        await categoryService.DeleteCategory(id);
    }
}