using Buenaventura.Client.Services;
using Microsoft.AspNetCore.Components;
using Buenaventura.Shared;

namespace Buenaventura.Client.Pages;

public partial class InvestmentEdit(
    IInvestmentCategoriesApi categoriesApi,
    IInvestmentsApi investmentsApi,
    AccountSyncService accountSyncService,
    NavigationManager navigationManager
    ) : ComponentBase
{
    private IEnumerable<InvestmentCategoryModel> categories = [];
    [CascadingParameter] IEnumerable<AccountWithBalance> accounts { get; set; } = [];
    [Parameter] public Guid? InvestmentId { get; set; }
    private decimal? total;
    private AddInvestmentModel investment = new()
    {
        Currency = "USD",
        Date = DateTime.Now,
    };
    private bool IsEditMode => InvestmentId.HasValue;
    private string PageTitle => IsEditMode ? "Edit Investment" : "Add New Investment";

    protected override async Task OnInitializedAsync()
    {
        categories = await categoriesApi.GetCategories();
        if (InvestmentId.HasValue)
        {
            var existingInvestment = await investmentsApi.GetInvestment(InvestmentId.Value);
            investment = new AddInvestmentModel
            {
                CategoryId = existingInvestment.CategoryId == Guid.Empty ? null : existingInvestment.CategoryId,
                Currency = existingInvestment.Currency,
                DontRetrievePrices = existingInvestment.DontRetrievePrices,
                Name = existingInvestment.Name,
                PaysDividends = existingInvestment.PaysDividends,
                Symbol = existingInvestment.Symbol,
            };
        }
    }

    private void CalculateTotal()
    {
        total = investment.Shares * investment.Price;
    }

    private void CalculatePrice()
    {
        if (total.HasValue && investment.Shares != 0)
        {
            investment.Price = total.Value / investment.Shares;
        }
    }

    private void Cancel()
    {
        navigationManager.NavigateTo("/Investments");
    }

    private async Task Save()
    {
        if (InvestmentId.HasValue)
        {
            await investmentsApi.UpdateInvestment(new UpdateInvestmentModel
            {
                CategoryId = investment.CategoryId,
                Currency = investment.Currency,
                DontRetrievePrices = investment.DontRetrievePrices,
                InvestmentId = InvestmentId.Value,
                Name = investment.Name,
                PaysDividends = investment.PaysDividends,
                Symbol = investment.Symbol,
            });
        }
        else
        {
            await investmentsApi.AddInvestment(investment);
            await accountSyncService.RefreshAccounts();
        }

        navigationManager.NavigateTo("/Investments");
    }
} 
