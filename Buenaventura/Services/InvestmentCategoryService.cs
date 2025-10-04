using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IInvestmentCategoryService : IAppService
{
    Task<IEnumerable<InvestmentCategoryModel>> GetCategories();
}

public class InvestmentCategoryService(
    BuenaventuraDbContext context
        ) : IInvestmentCategoryService
{
    public async Task<IEnumerable<InvestmentCategoryModel>> GetCategories()
    {
        return await context.InvestmentCategories.Select(c => new InvestmentCategoryModel
        {
            InvestmentCategoryId = c.InvestmentCategoryId,
            Name = c.Name,
            Percentage = c.Percentage,
        }).ToListAsync();
    }
}