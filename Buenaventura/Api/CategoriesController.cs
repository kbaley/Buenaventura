using Buenaventura.Data;
using Buenaventura.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CategoriesController(CoronadoDbContext context) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Category> GetCategory()
    {
        return context.Categories.OrderBy(c => c.Name);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutCategory([FromRoute] Guid id, [FromBody] Category category)
    {
        context.Entry(category).State = EntityState.Modified;
        await context.SaveChangesAsync();

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> PostCategory([FromBody] Category category)
    {
        if (category.CategoryId == Guid.Empty) category.CategoryId = Guid.NewGuid();
        context.Categories.Add(category);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return CreatedAtAction("PostCategory", new { id = category.CategoryId }, category);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
    {
        var category = await context.Categories.FindAsync(id).ConfigureAwait(false);
        if (category == null) {
            return NotFound();
        }
        context.Categories.Remove(category);
        await context.SaveChangesAsync().ConfigureAwait(false);
        return Ok(category);
    }
}