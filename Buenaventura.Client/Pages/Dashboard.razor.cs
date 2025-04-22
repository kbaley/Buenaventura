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
    private ApexChart<IncomeExpenseDataPoint> incomeExpenseChart;
    private ApexChart<ReportDataPoint> netWorthChart;

    private ApexChartOptions<ReportDataPoint> netWorthChartOptions = ChartOptions.GetDefaultOptions<ReportDataPoint>();
    private ApexChartOptions<LineChartDataPoint> investmentsChartOptions = ChartOptions.GetDefaultOptions<LineChartDataPoint>();
    private ApexChartOptions<IncomeExpenseDataPoint> incomeExpenseChartOptions =
        ChartOptions.GetDefaultOptions<IncomeExpenseDataPoint>();

    protected override async Task OnParametersSetAsync()
    {
        expensesThisMonth = await dashboardService.GetThisMonthExpenses();
        creditCardBalance = await dashboardService.GetCreditCardBalance();
        liquidAssetBalance = await dashboardService.GetLiquidAssetBalance();
        incomeExpenseData = await dashboardService.GetIncomeExpenseData();
        netWorthData = await dashboardService.GetNetWorthData();
        StateHasChanged();
        if (incomeExpenseChart != null)
        {
            await incomeExpenseChart.UpdateSeriesAsync(false);
        }

        if (netWorthChart != null)
        {
            await netWorthChart.UpdateSeriesAsync(false);
        }

        await base.OnParametersSetAsync();
    }

    private readonly List<ExpenseDataPoint> expenseData =
    [
        new() { Category = "Housing", Amount = 35 },
        new() { Category = "Food", Amount = 25 },
        new() { Category = "Transportation", Amount = 15 },
        new() { Category = "Entertainment", Amount = 10 },
        new() { Category = "Utilities", Amount = 15 },
        new() { Category = "Other", Amount = 18 }
    ];

    private readonly List<LineChartDataPoint> investmentData =
    [
        new() { Month = "May 2024", Amount = 10000 },
        new() { Month = "Jun 2024", Amount = 12000 },
        new() { Month = "Jul 2024", Amount = 15000 },
        new() { Month = "Aug 2024", Amount = 13000 },
        new() { Month = "Sep 2024", Amount = 14000 },
        new() { Month = "Oct 2024", Amount = 16000 },
        new() { Month = "Nov 2024", Amount = 17000 },
        new() { Month = "Dec 2024", Amount = 18000 },
        new() { Month = "Jan 2025", Amount = 19000 },
        new() { Month = "Feb 2025", Amount = 20000 },
        new() { Month = "Mar 2025", Amount = 21000 },
        new() { Month = "Apr 2025", Amount = 22000 }
    ];

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

    public class LineChartDataPoint
    {
        public string Month { get; set; } = "";
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