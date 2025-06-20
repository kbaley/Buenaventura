﻿using AutoMapper;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class InvestmentCategoriesController(BuenaventuraDbContext context, IMapper mapper,
    IInvestmentCategoryService investmentCategoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<InvestmentCategoryModel>> GetCategories()
    {
        return await investmentCategoryService.GetCategories();
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
                    await context.InvestmentCategories.RemoveByIdAsync(category.InvestmentCategoryId);
                    break;
                case "added":
                    await context.InvestmentCategories.AddAsync(mappedCategory);
                    break;

            }
        }
        await context.SaveChangesAsync();

        return Ok(context.InvestmentCategories);
    }
}