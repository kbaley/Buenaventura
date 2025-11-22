using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Dtos;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

public class UpdateInvestmentCategories(BuenaventuraDbContext context)
    : Endpoint<IEnumerable<InvestmentCategoryForUpdate>, IEnumerable<InvestmentCategory>>
{
    public override void Configure()
    {
        Post("/api/investmentcategories");
    }

    public override async Task HandleAsync(IEnumerable<InvestmentCategoryForUpdate> req, CancellationToken ct)
    {
        var categories = req.ToArray();
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
            var mappedCategory = category.ToInvestmentCategory();
            switch (category.Status.ToLower())
            {
                case "updated":
                    context.Entry(mappedCategory).State = EntityState.Modified;
                    break;
                case "deleted":
                    await context.InvestmentCategories.RemoveByIdAsync(category.InvestmentCategoryId);
                    break;
                case "added":
                    await context.InvestmentCategories.AddAsync(mappedCategory, ct);
                    break;

            }
        }
        await context.SaveChangesAsync(ct);
        await SendOkAsync(context.InvestmentCategories.ToArray(), ct);
    }
}