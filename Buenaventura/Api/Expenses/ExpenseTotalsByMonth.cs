using Buenaventura.Services;
using Buenaventura.Domain;
using Buenaventura.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;

namespace Buenaventura.Api;

internal class ExpenseTotalsByMonth(IExpenseService expenseService, UserManager<User> userManager)
    : Endpoint<ExpenseReportRequest, CategoryTotals>
{
    public override void Configure()
    {
        Get("/api/expenses/totalsbymonth");
    }

    public override async Task HandleAsync(ExpenseReportRequest req, CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(User);
        var data = await expenseService.GetExpenseTotalsByMonth(
            TransactionTagFormatter.ParseTagText(req.IncludeTags),
            TransactionTagFormatter.ParseTagText(req.ExcludeTags),
            req.AllTime,
            isRestricted: user?.Restricted ?? false);
        await Send.OkAsync(data, ct);
    }
}
