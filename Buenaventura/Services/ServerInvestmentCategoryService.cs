using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerInvestmentCategoryService(IDbContextFactory<BuenaventuraDbContext> dbContextFactory) : IInvestmentCategoryService
{
    public async Task<IEnumerable<InvestmentCategoryModel>> GetCategories()
    {
        var context = await dbContextFactory.CreateDbContextAsync();
        return context.InvestmentCategories.Select(c => new InvestmentCategoryModel
        {
            InvestmentCategoryId = c.InvestmentCategoryId,
            Name = c.Name,
            Percentage = c.Percentage,
        });
    }
}