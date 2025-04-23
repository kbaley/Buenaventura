using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerInvestmentCategoryService(
    BuenaventuraDbContext context
        ) : IInvestmentCategoryService
{
    public async Task<IEnumerable<InvestmentCategoryModel>> GetCategories()
    {
        return context.InvestmentCategories.Select(c => new InvestmentCategoryModel
        {
            InvestmentCategoryId = c.InvestmentCategoryId,
            Name = c.Name,
            Percentage = c.Percentage,
        });
    }
}