using Buenaventura.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Buenaventura.Functions;

public class RefreshPricesTimer(ILogger<RefreshPricesTimer> logger, IInvestmentService investmentService)
{
    [Function("RefreshPricesTimer")]
    public async Task Run([TimerTrigger("0 45 14 * * 1-5", RunOnStartup = true)] TimerInfo myTimer)
    {
        logger.LogInformation("C# Timer trigger function executed at: {Time}", DateTime.Now);

        if (myTimer.ScheduleStatus is not null)
        {
            logger.LogInformation("Next timer schedule at: {NextTime}", myTimer.ScheduleStatus.Next);
        }

        try
        {
            logger.LogInformation("Starting price update...");
            var result = await investmentService.UpdateCurrentPrices();
            var count = result.Investments.Count();
            logger.LogInformation("Price update completed. Updated {Count} investments.", count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating prices");
            throw;
        }
    }
}