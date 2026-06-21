using System.ComponentModel;
using Buenaventura.Services;
using Buenaventura.Shared;
using ModelContextProtocol.Server;

namespace Buenaventura.MCP;

[McpServerToolType]
public static class FinancialTools
{
    [McpServerTool(Name = "get_transactions", ReadOnly = true, Idempotent = true)]
    [Description("Returns income or expense transactions in a date range. Dates are inclusive at the start and exclusive at the end. Amounts are positive and expressed in Buenaventura's base currency.")]
    public static Task<IReadOnlyList<FinancialTransactionResult>> GetTransactions(
        IFinancialQueryService service,
        [Description("Inclusive start date in YYYY-MM-DD format.")] DateTime startDate,
        [Description("Exclusive end date in YYYY-MM-DD format.")] DateTime endDate,
        [Description("Optional: Income or Expense.")] string? type = null,
        [Description("Optional case-insensitive text to find in vendor, description, or category.")] string? search = null,
        [Description("Maximum number of results, from 1 to 200.")] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(startDate, endDate);
        return service.GetTransactions(startDate, endDate, type, search, Math.Clamp(limit, 1, 200), cancellationToken);
    }

    [McpServerTool(Name = "summarize_finances", ReadOnly = true, Idempotent = true)]
    [Description("Summarizes income and expenses by category for a date range. Dates are inclusive at the start and exclusive at the end. Amounts are positive and expressed in Buenaventura's base currency.")]
    public static Task<IReadOnlyList<FinancialSummaryResult>> SummarizeFinances(
        IFinancialQueryService service,
        [Description("Inclusive start date in YYYY-MM-DD format.")] DateTime startDate,
        [Description("Exclusive end date in YYYY-MM-DD format.")] DateTime endDate,
        [Description("Optional: Income or Expense.")] string? type = null,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(startDate, endDate);
        return service.GetSummary(startDate, endDate, type, cancellationToken);
    }

    [McpServerTool(Name = "list_financial_categories", ReadOnly = true, Idempotent = true)]
    [Description("Lists the income and expense categories configured in Buenaventura.")]
    public static Task<IReadOnlyList<FinancialCategoryResult>> ListFinancialCategories(
        IFinancialQueryService service,
        [Description("Optional: Income or Expense.")] string? type = null,
        CancellationToken cancellationToken = default) =>
        service.GetCategories(type, cancellationToken);

    private static void ValidateDateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
        {
            throw new ArgumentException("endDate must be later than startDate.");
        }

        if (endDate - startDate > TimeSpan.FromDays(366))
        {
            throw new ArgumentException("A query may cover at most 366 days.");
        }
    }
}
