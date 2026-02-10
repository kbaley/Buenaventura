A port of [Coronado](https://github.com/kbaley/coronado) to Blazor using (hopefully) the same API

## RefreshTimer Azure Function

This function runs every weekday at 2:45 PM UTC and retrieves the latest investment data, similar to what
happens when you refresh the prices manually on the Investments page. It is set to 2:45 PM UTC rather than using
WEBSITE_TIME_ZONE because Azure Functions does not support setting the time zone for Linux consumption plans.

See [this article](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?pivots=programming-language-python&tabs=python-v2%2Cin-process%2Cnodejs-v4#ncrontab-time-zones) for details.

## Deploying

Done via a GitHub Action with azure-webapps-dotnet-core.yml.

## Azure App Service notes

Set the Startup Command to `dotnet Buenaventura.dll`. This became required after updating the application to .NET 10. 
I _think_ it's because there are two runtimeconfig.json files and this helps determine which one to launch but this
command wasn't required when it was .NET 9 so 🤷‍♂️.


## Monitoring

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

## EF migrations

To add a new migration, use the following command in the terminal from the root folder:

```
dotnet ef migrations add <MigrationName> --project Buenaventura.Domain/Buenaventura.Domain.csproj --startup-project Buenaventura/Buenaventura.csproj
```

To update the database:

```
dotnet ef database update --project Buenaventura.Domain/Buenaventura.Domain.csproj --startup-project Buenaventura/Buenaventura.csproj
```
