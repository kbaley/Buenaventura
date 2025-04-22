using ApexCharts;

namespace Buenaventura.Client.Services;

public class ChartOptions
{
    public static ApexChartOptions<T> GetDefaultOptions<T>() where T : class
    {
        return new ApexChartOptions<T>
        {
            Tooltip = new Tooltip
            {
                Y = new TooltipY
                {
                    // The Y-axis formatter will apply to the tooltip by default 
                    // but we want to see the raw value in the tooltip
                    Formatter =
                        "(x) => x.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })"
                }
            },
            Yaxis =
            [
                new YAxis()
                {
                    Labels = new YAxisLabels
                    {
                        Formatter = "formatMoneyAxis"
                    }
                }
            ]
        };
    }
}