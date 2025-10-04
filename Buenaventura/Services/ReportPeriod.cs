using Buenaventura.Api;
using Buenaventura.Domain;

namespace Buenaventura.Services;

/// <summary>
/// Dates for a reporting period
///
/// The end date is meant to be exclusive, so the period is [start, end)
/// </summary>
public class ReportPeriod
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    
    public static ReportPeriod GetLast12Months()
    {
        var today = DateTime.Today;
        var end = today.FirstDayOfMonth().AddMonths(1);
        var start = today.AddMonths(-11).FirstDayOfMonth();
        return new ReportPeriod
        {
            Start = start,
            End = end
        };
    }
    
    /// <summary>
    /// Get the 12-month period ending the first day of this month
    /// </summary>
    /// <returns></returns>
    public static ReportPeriod GetLast12MonthsFromLastMonth()
    {
        var today = DateTime.Today;
        var end = today.FirstDayOfMonth();
        var start = today.AddMonths(-12).FirstDayOfMonth();
        return new ReportPeriod
        {
            Start = start,
            End = end
        };
    }
}