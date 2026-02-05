using Buenaventura.Domain;
using Buenaventura.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using IAccountService = Buenaventura.Services.IAccountService;

namespace Buenaventura.Api;

internal record DownloadTransactionsCsvRequest(Guid AccountId, int Page = 0, int PageSize = 50, string? Search = null);

internal class DownloadTransactionsCsv(IAccountService accountService, UserManager<User> userManager)
    : Endpoint<DownloadTransactionsCsvRequest, TransactionListModel>
{
    public override void Configure()
    {
        Get($"/api/accounts/{{AccountId}}/transactions/csv");
    }

    public override async Task HandleAsync(DownloadTransactionsCsvRequest request, CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(User);
        var isRestricted = user?.Restricted ?? false;
        var account = await accountService.GetAccount(request.AccountId);
        var transactions = await accountService.GetTransactions(request.AccountId, "", 0, int.MaxValue, isRestricted);

        var csvContent = "Date,Vendor,Category,Description,Debit,Credit,Balance\n";
        foreach (var transaction in transactions.Items)
        {
            csvContent += $"{transaction.TransactionDate:MM/dd/yyyy}," +
                         $"\"{transaction.Vendor?.Replace("\"", "\"\"")}\"," +
                         $"\"{transaction.Category.Name.Replace("\"", "\"\"")}\"," +
                         $"\"{transaction.Description?.Replace("\"", "\"\"")}\"," +
                         $"{transaction.Debit?.ToString() ?? ""}," +
                         $"{transaction.Credit?.ToString() ?? ""}," +
                         $"{transaction.RunningTotal}\n";
        }

        var fileName = $"{account.Name.Replace(" ", "_")}_transactions_{DateTime.Now:yyyy-MM-dd}.csv";
        var fileContentBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        await SendBytesAsync(fileContentBytes, fileName, "text/csv", cancellation: ct);
    }
}