using Buenaventura.Api;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;

namespace Buenaventura.Services;

public class ServerDashboardService(IReportRepository reportRepo) : IDashboardService
{
    public async Task<IEnumerable<ReportDataPoint>> GetNetWorthData(int? year = null)
    {
        year ??= DateTime.Today.Year;
        var netWorth = new List<ReportDataPoint>();

        var date = year.Value.GetEndDateForYear();
        var numItems = DateTime.Today.Month + 1;
        if (year != DateTime.Today.Year) {
            numItems = 13;
        }
        for (var i = 0; i < numItems; i++) {
            netWorth.Add(new ReportDataPoint
            {
                Label = date.ToString("MMM yy"), 
                Value = await reportRepo.GetNetWorthFor(date)
            });
            date = date.FirstDayOfMonth().AddMinutes(-1);
        }
        return netWorth;
    }
}