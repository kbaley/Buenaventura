using AutoMapper;
using Buenaventura.Data;
using Buenaventura.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class InvestmentCategoriesController(BuenaventuraDbContext context, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public IEnumerable<InvestmentCategory> GetCategories()
    {
        return context.InvestmentCategories.OrderBy(c => c.Name);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCategories([FromBody] InvestmentCategoryForUpdate[] categories)
    {
        // Update any investments that refer to a deleted category to remove the reference to the category
        foreach (var investment in context.Investments)
        {
            if (categories.Any(c => c.Status.Equals("deleted", StringComparison.InvariantCultureIgnoreCase)
                                    && investment.CategoryId == c.InvestmentCategoryId))
            {
                investment.CategoryId = null;
                investment.Category = null;
            }
        }

        // now update the categories
        foreach (var category in categories)
        {
            var mappedCategory = mapper.Map<InvestmentCategory>(category);
            switch (category.Status.ToLower())
            {
                case "updated":
                    context.Entry(mappedCategory).State = EntityState.Modified;
                    break;
                case "deleted":
                    await context.InvestmentCategories.RemoveByIdAsync(category.InvestmentCategoryId).ConfigureAwait(false);
                    break;
                case "added":
                    await context.InvestmentCategories.AddAsync(mappedCategory).ConfigureAwait(false);
                    break;

            }
        }
        await context.SaveChangesAsync().ConfigureAwait(false);

        return Ok(context.InvestmentCategories);
    }
}