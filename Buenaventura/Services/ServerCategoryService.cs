using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerCategoryService(
    IDbContextFactory<CoronadoDbContext> dbContextFactory
) : ICategoryService
{
    public async Task<IEnumerable<CategoryModel>> GetCategories()
    {
        var context = await dbContextFactory.CreateDbContextAsync();
        var categories = await context.Categories.OrderBy(c => c.Name).ToListAsync();
        return categories.Select(c => new CategoryModel
        {
            CategoryId = c.CategoryId,
            Name = c.Name,
            Type = CategoryType.REGULAR
        });
    }
}