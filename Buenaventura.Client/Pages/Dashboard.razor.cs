using Buenaventura.Client.Services;
using Buenaventura.Shared;

namespace Buenaventura.Client.Pages;

public partial class Dashboard(IDashboardService dashboardService)
{
    private decimal creditCardBalance;
    private decimal liquidAssetBalance;
    private decimal expensesThisMonth;
    private IEnumerable<IncomeExpenseDataPoint> incomeExpenseData = [];
    private IEnumerable<ReportDataPoint> netWorthData = [];
    private IEnumerable<ReportDataPoint> investmentData = [];
    private IEnumerable<ReportDataPoint> expenseData = [];
    private IEnumerable<ReportDataPoint> assetData = [];
    private IEnumerable<ExpenseAveragesDataPoint> expenseAveragesData = [];
    private bool isLoading = true;

    protected override async Task OnParametersSetAsync()
    {
        isLoading = true;

        var expensesTask = dashboardService.GetThisMonthExpenses();
        var creditCardTask = dashboardService.GetCreditCardBalance();
        var liquidAssetTask = dashboardService.GetLiquidAssetBalance();
        var incomeExpenseTask = dashboardService.GetIncomeExpenseData();
        var netWorthTask = dashboardService.GetNetWorthData();
        var investmentTask = dashboardService.GetInvestmentData();
        var expenseTask = dashboardService.GetExpenseCategoryBreakdown();
        var assetTask = dashboardService.GetAssetClassData();
        var expenseAveragesTask = dashboardService.GetExpenseAveragesData();

        await Task.WhenAll(
            expensesTask,
            creditCardTask,
            liquidAssetTask,
            incomeExpenseTask,
            netWorthTask,
            investmentTask,
            expenseTask,
            assetTask,
            expenseAveragesTask
        );

        expensesThisMonth = -(await expensesTask);
        creditCardBalance = await creditCardTask;
        liquidAssetBalance = await liquidAssetTask;
        incomeExpenseData = await incomeExpenseTask;
        netWorthData = await netWorthTask;
        investmentData = await investmentTask;
        expenseData = await expenseTask;
        assetData = await assetTask;
        expenseAveragesData = await expenseAveragesTask;
        StateHasChanged();
        isLoading = false;

        await base.OnParametersSetAsync();
    }

}