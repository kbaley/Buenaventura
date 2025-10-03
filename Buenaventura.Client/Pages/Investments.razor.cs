using Buenaventura.Client.Components;
using Buenaventura.Client.Services;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Buenaventura.Client.Pages;

public partial class Investments(
    IInvestmentsApi investmentsApi,
    NavigationManager navigationManager,
    AccountSyncService accountSyncService,
    IDialogService dialogService
    ) : ComponentBase
{

    [CascadingParameter] IEnumerable<AccountWithBalance> accounts { get; set; } = [];
    private IEnumerable<InvestmentModel>? investments;
    private double portfolioIrr;

    protected override async Task OnInitializedAsync()
    {
        await LoadInvestments();
    }

    private async Task LoadInvestments()
    {
        var investmentList = await investmentsApi.GetInvestments();
        investments = investmentList.Investments;
        portfolioIrr = investmentList.PortfolioIrr;
    }

    private void EditInvestment(InvestmentModel investment)
    {
        navigationManager.NavigateTo($"/Investments/Edit/{investment.InvestmentId}");
    }

    private async Task DeleteInvestment(InvestmentModel investment)
    {
        var parameters = new DialogParameters
        {
            ["ContentText"] = $"Are you sure you want to delete investment {investment.Symbol}?",
            ["ButtonText"] = "Delete",
            ["Color"] = Color.Error
        };

        var dialog = await dialogService.ShowAsync<ConfirmationDialog>("Delete Investment", parameters);
        var result = await dialog.Result;
        if (!result!.Canceled)
        {
            parameters = new DialogParameters
            {
                ["ContentText"] = $@"
SERIOUSLY. Are you VERY sure you want to delete investment {investment.Symbol}?
This is a very destructive action and will delete all transactions related to this investment.
Do this only for test databases or if you are absolutely sure you want to delete this investment.",
                ["ButtonText"] = "DELETE!!!",
                ["Color"] = Color.Error,
            };
            dialog = await dialogService.ShowAsync<ConfirmationDialog>("Delete Investment", parameters);
            result = await dialog.Result;
            if (!result!.Canceled)
            {
                await investmentsApi.DeleteInvestment(investment.InvestmentId);
                await LoadInvestments();
            }
        }
    }

    private async Task BuySellInvestment(InvestmentModel investment, bool isBuy)
    {
        var parameters = new DialogParameters
        {
            ["Investment"] = investment,
            ["IsBuy"] = isBuy,
            ["Accounts"] = accounts
        };

        var dialog = await dialogService.ShowAsync<BuySellDialog>(isBuy ? "Buy Investment" : "Sell Investment", parameters);
        var result = await dialog.Result;
        if (!result!.Canceled)
        {
            var transaction = (BuySellModel)result.Data!;
            await investmentsApi.BuySell(transaction);
            await accountSyncService.RefreshAccounts();
            await LoadInvestments();
        }
    }

    private async Task RecordDividend(InvestmentModel investment)
    {
        var parameters = new DialogParameters
        {
            ["Accounts"] = accounts,
            ["Investment"] = investment
        };

        var dialog = await dialogService.ShowAsync<DividendDialog>("Record Dividend", parameters);
        var result = await dialog.Result;
        if (!result!.Canceled)
        {
            var model = (RecordDividendModel)result.Data!;
            model.InvestmentId = investment.InvestmentId;
            await investmentsApi.RecordDividend(model);
            await accountSyncService.RefreshAccounts();
            await LoadInvestments();
        }
    }

    private async Task UpdatePrices()
    {
        var investmentList = await investmentsApi.UpdateCurrentPrices();
        investments = investmentList.Investments;
        portfolioIrr = investmentList.PortfolioIrr;
        StateHasChanged();
    }

    private async Task SyncPortfolioValue()
    {
        await investmentsApi.MakeCorrectingEntry();
        await accountSyncService.RefreshAccounts();
    }

    private void ComparePortfolioRatios()
    {
        navigationManager.NavigateTo("/Investments/Ratios");
    }

    private void AddInvestment()
    {
        navigationManager.NavigateTo("/Investments/new");
    }
}