using System.Net;
using Buenaventura.Client.Services;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Components;
using Refit;

namespace Buenaventura.Client.Pages;

public partial class InvestmentDetails(IInvestmentsApi investmentsApi) : ComponentBase
{
    [Parameter] public Guid InvestmentId { get; set; }

    private InvestmentDetailDto? investment;
    private IEnumerable<InvestmentTransactionDto> transactions = [];
    private bool isLoading = true;
    private string? errorMessage;

    protected override async Task OnParametersSetAsync()
    {
        isLoading = true;
        errorMessage = null;
        investment = null;
        transactions = [];

        try
        {
            investment = await investmentsApi.GetInvestment(InvestmentId);
            transactions = investment.Transactions.OrderByDescending(t => t.Date).ToList();
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            errorMessage = "This investment could not be found.";
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            errorMessage = "Unable to load investment details. Please try again later.";
        }
        finally
        {
            isLoading = false;
        }
    }
}
