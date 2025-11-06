using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Buenaventura.Functions;

public class RefreshPricesTimer
{
    private readonly ILogger<RefreshPricesTimer> _logger;

    public RefreshPricesTimer(ILogger<RefreshPricesTimer> logger)
    {
        _logger = logger;
    }

    [Function("RefreshPricesTimer")]
    public void Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            
        }
    }
}