using Buenaventura.Client.Services;
using Microsoft.AspNetCore.Components;
using Buenaventura.Shared;

namespace Buenaventura.Client.Pages;

public partial class InvestmentEdit(
    IInvestmentCategoryService categoryService,
    IAccountService accountService,
    NavigationManager navigationManager
    ) : ComponentBase
{
    private IEnumerable<InvestmentCategoryModel> categories = [];
    private IEnumerable<AccountWithBalance> accounts = [];
    private decimal? shares;
    private decimal? price;
    private decimal? total;
    private string name = "";
    private string symbol = "";
    private string currency = "USD";
    private bool includePrices = true;
    private bool paysDividends;
    private Guid? categoryId;
    private DateTime? purchaseDate = DateTime.Today;
    private Guid? debitAccountId;

    protected override async Task OnInitializedAsync()
    {
        categories = await categoryService.GetCategories();
        accounts = await accountService.GetAccounts();
    }

    private void CalculateTotal()
    {
        if (shares.HasValue && price.HasValue)
        {
            total = shares.Value * price.Value;
        }
    }

    private void CalculatePrice()
    {
        if (shares.HasValue && shares.Value > 0 && total.HasValue)
        {
            price = total.Value / shares.Value;
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