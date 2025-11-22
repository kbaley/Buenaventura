A port of [Coronado](https://github.com/kbaley/coronado) to Blazor using (hopefully) the same API

## RefreshTimer Azure Function

This function runs every weekday at 2:45 PM UTC and retrieves the latest investment data, similar to what
happens when you refresh the prices manually on the Investments page. It is set to 2:45 PM UTC rather than using
WEBSITE_TIME_ZONE because Azure Functions does not support setting the time zone for Linux consumption plans.

See [this article](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?pivots=programming-language-python&tabs=python-v2%2Cin-process%2Cnodejs-v4#ncrontab-time-zones) for details.

### Deploying

Currently done directly from Rider in Azure Explorer. In the future, may set up a GitHub Action to deploy on push to
main but as of November 2025, this is not yet done.

### Monitoring

Go to the Azure Portal, navigate to the Function App, and select "Monitoring" -> "Logs" to see the logs for the function.

In "Tables", select "exceptions" to see any exceptions that have occurred during function execution.

Use this to see the result of recent runs:

```
traces
| where timestamp > ago(1h)
| where message contains "RefreshPricesTimer" or customDimensions.Category contains "RefreshPricesTimer"
| order by timestamp desc
```

To trigger the function manually, navigate to it in Azure Explorer in Rider. Right-click on the function and select
"Trigger Function with Http Client".