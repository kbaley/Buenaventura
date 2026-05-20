using Buenaventura.Client.Services;
using Buenaventura.Shared;
using MudBlazor;

namespace Buenaventura.Client.Pages;

public partial class Dashboard(
    IDashboardApi dashboardApi,
    IExpensesApi expensesApi,
    IReimbursementsApi reimbursementsApi)
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
    private ReimbursementSummary reimbursementSummary = new();
    private bool isLoading = true;

    protected override async Task OnParametersSetAsync()
    {
        isLoading = true;

        var expensesTask = expensesApi.GetThisMonthExpenses();
        var creditCardTask = dashboardApi.GetCreditCardBalance();
        var liquidAssetTask = dashboardApi.GetLiquidAssetBalance();
        var incomeExpenseTask = dashboardApi.GetIncomeExpenseData();
        var netWorthTask = dashboardApi.GetNetWorthData();
        var investmentTask = dashboardApi.GetInvestmentData();
        var expenseTask = expensesApi.GetExpenseCategoryBreakdown();
        var assetTask = dashboardApi.GetAssetClassData();
        var expenseAveragesTask = expensesApi.GetExpenseAveragesData();
        var reimbursementSummaryTask = reimbursementsApi.GetSummary();

        await Task.WhenAll(
            expensesTask,
            creditCardTask,
            liquidAssetTask,
            incomeExpenseTask,
            netWorthTask,
            investmentTask,
            expenseTask,
            assetTask,
            expenseAveragesTask,
            reimbursementSummaryTask
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
        reimbursementSummary = await reimbursementSummaryTask;
        StateHasChanged();
        isLoading = false;

        await base.OnParametersSetAsync();
    }

    private Color GetReimbursementColor()
    {
        return reimbursementSummary.OutstandingBalance switch
        {
            > 0 => Color.Warning,
            < 0 => Color.Info,
            _ => Color.Success
        };
    }

}
