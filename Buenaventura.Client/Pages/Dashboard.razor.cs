using ApexCharts;
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
    private ApexChart<ReportDataPoint> netWorthChart;
    private ApexChart<ReportDataPoint> expenseChart;
    private bool isLoading = true;

    private ApexChartOptions<ReportDataPoint> netWorthChartOptions = ChartOptions.GetDefaultOptions<ReportDataPoint>();


    protected override async Task OnParametersSetAsync()
    {
        isLoading = true;
        expensesThisMonth = await dashboardService.GetThisMonthExpenses();
        creditCardBalance = await dashboardService.GetCreditCardBalance();
        liquidAssetBalance = await dashboardService.GetLiquidAssetBalance();
        incomeExpenseData = await dashboardService.GetIncomeExpenseData();
        netWorthData = await dashboardService.GetNetWorthData();
        investmentData = await dashboardService.GetInvestmentData();
        expenseData = await dashboardService.GetExpenseData();
        StateHasChanged();
        if (netWorthChart != null)
        {
            await netWorthChart.UpdateSeriesAsync(false);
        }
        if (expenseChart != null)
        {
            await expenseChart.UpdateSeriesAsync(false);
        }

        isLoading = false;

        await base.OnParametersSetAsync();
    }

    private readonly List<ExpenseDataPoint> assetData =
    [
        new() { Category = "Bank Accounts", Amount = 35 },
        new() { Category = "Investments", Amount = 25 },
        new() { Category = "Assets", Amount = 15 },
    ];

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

    public class ExpenseDataPoint
    {
        public string Category { get; set; } = "";
        public decimal Amount { get; set; }
    }

    private class Top5ExpenseDataPoint
    {
        public string Category { get; set; } = "";
        public decimal CurrentMonth { get; set; }
        public decimal Previous3MonthsAverage { get; set; }
        public decimal Previous12MonthsAverage { get; set; }
    }
}