using Buenaventura.Client.Services;
using Microsoft.AspNetCore.Components;
using Buenaventura.Shared;

namespace Buenaventura.Client.Pages;

public partial class InvestmentEdit(
    IInvestmentCategoryService categoryService,
    NavigationManager navigationManager
    ) : ComponentBase
{
    private IEnumerable<InvestmentCategoryModel> categories = [];
    [CascadingParameter] IEnumerable<AccountWithBalance> accounts { get; set; } = [];
    private decimal? total;
    private readonly AddInvestmentModel investment = new AddInvestmentModel
    {
        Currency = "USD"
    };

    protected override async Task OnInitializedAsync()
    {
        categories = await categoryService.GetCategories();
    }

    private void CalculateTotal()
    {
        total = investment.Shares * investment.Price;
    }

    private void CalculatePrice()
    {
        if (total.HasValue)
        {
            investment.Price = total.Value / investment.Shares;
        }
    }

    private void Cancel()
    {
        navigationManager.NavigateTo("/Investments");
    }

    private void Save()
    {
        // Save functionality will be implemented later
        navigationManager.NavigateTo("/Investments");
    }
} 