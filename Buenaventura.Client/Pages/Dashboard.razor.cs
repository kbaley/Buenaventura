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
        var expenseTask = dashboardService.GetExpenseData();
        var assetTask = dashboardService.GetAssetClassData();

        await Task.WhenAll(
            expensesTask,
            creditCardTask,
            liquidAssetTask,
            incomeExpenseTask,
            netWorthTask,
            investmentTask,
            expenseTask,
            assetTask
        );

        expensesThisMonth = await expensesTask;
        creditCardBalance = await creditCardTask;
        liquidAssetBalance = await liquidAssetTask;
        incomeExpenseData = await incomeExpenseTask;
        netWorthData = await netWorthTask;
        investmentData = await investmentTask;
        expenseData = await expenseTask;
        assetData = await assetTask;
        StateHasChanged();
        isLoading = false;

        await base.OnParametersSetAsync();
    }

    private List<Top5ExpenseDataPoint> topExpensesData =
    [
        new()
        {
            Category = "Housing", CurrentMonth = 1200, Previous3MonthsAverage = 1100, Previous12MonthsAverage = 1000
        },
        new() { Category = "Food", CurrentMonth = 800, Previous3MonthsAverage = 700, Previous12MonthsAverage = 600 },
        new()
        {
            Category = "Transportation", CurrentMonth = 600, Previous3MonthsAverage = 500, Previous12MonthsAverage = 400
        },
        new()
        {
            Category = "Entertainment", CurrentMonth = 400, Previous3MonthsAverage = 350, Previous12MonthsAverage = 300
        },
        new()
        {
            Category = "Utilities", CurrentMonth = 300, Previous3MonthsAverage = 250, Previous12MonthsAverage = 200
        }
    ];

    private class Top5ExpenseDataPoint
    {
        public string Category { get; set; } = "";
        public decimal CurrentMonth { get; set; }
        public decimal Previous3MonthsAverage { get; set; }
        public decimal Previous12MonthsAverage { get; set; }
    }
}