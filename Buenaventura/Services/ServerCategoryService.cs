using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerCategoryService(
    IDbContextFactory<BuenaventuraDbContext> dbContextFactory
) : ICategoryService
{
    public async Task<IEnumerable<CategoryModel>> GetCategories()
    {
        var context = await dbContextFactory.CreateDbContextAsync();
        var categories = await context.Categories
            .GroupJoin(context.Transactions,
                category => category.CategoryId,
                transaction => transaction.CategoryId,
                (category, transaction) => new CategoryModel
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Type = CategoryType.REGULAR,
                    TimesUsed = transaction.Count()
                })
            .OrderByDescending(c => c.TimesUsed)
            .ToListAsync();
        return categories;
    }
}