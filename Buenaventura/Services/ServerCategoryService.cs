using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerCategoryService(
    BuenaventuraDbContext context
) : ICategoryService
{
    public async Task<IEnumerable<CategoryModel>> GetCategories()
    {
        var categories = await context.Categories
            .GroupJoin(context.Transactions,
                category => category.CategoryId,
                transaction => transaction.CategoryId,
                (category, transaction) => new CategoryModel
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Type = CategoryType.REGULAR,
                    TimesUsed = transaction.Count(),
                    CategoryClass = category.Type,
                    IncludeInReports = category.IncludeInReports
                })
            .OrderByDescending(c => c.TimesUsed)
            .ToListAsync();
        return categories;
    }

    public async Task DeleteCategory(Guid id)
    {
        await context.Categories.RemoveByIdAsync(id);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task UpdateCategory(CategoryModel categoryModel)
    {
        var category = await context.Categories.FindAsync(categoryModel.CategoryId);
        if (category == null)
        {
            throw new Exception("Category not found");
        }
        category.IncludeInReports = categoryModel.IncludeInReports;
        category.Name = categoryModel.Name;
        category.Type = categoryModel.CategoryClass;
        await context.SaveChangesAsync().ConfigureAwait(false);
    }
}