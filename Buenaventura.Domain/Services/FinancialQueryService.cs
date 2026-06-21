using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IFinancialQueryService : IAppService
{
    Task<IReadOnlyList<FinancialTransactionResult>> GetTransactions(
        DateTime startDate,
        DateTime endDate,
        string? type,
        string? search,
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FinancialSummaryResult>> GetSummary(
        DateTime startDate,
        DateTime endDate,
        string? type,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FinancialCategoryResult>> GetCategories(
        string? type,
        CancellationToken cancellationToken = default);
}

public sealed class FinancialQueryService(BuenaventuraDbContext context) : IFinancialQueryService
{
    public async Task<IReadOnlyList<FinancialTransactionResult>> GetTransactions(
        DateTime startDate,
        DateTime endDate,
        string? type,
        string? search,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizeType(type);
        var query = context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.TransactionDate >= startDate &&
                transaction.TransactionDate < endDate &&
                transaction.Category != null &&
                (transaction.Category.Type == "Expense" || transaction.Category.Type == "Income"));

        if (normalizedType is not null)
        {
            query = query.Where(transaction => transaction.Category!.Type == normalizedType);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(transaction =>
                EF.Functions.ILike(transaction.Vendor ?? "", pattern) ||
                EF.Functions.ILike(transaction.Description ?? "", pattern) ||
                EF.Functions.ILike(transaction.Category!.Name, pattern));
        }

        return await query
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.EnteredDate)
            .Take(limit)
            .Select(transaction => new FinancialTransactionResult(
                transaction.TransactionId,
                transaction.TransactionDate,
                transaction.Category!.Type,
                transaction.Category.Name,
                transaction.Account!.Name,
                transaction.Vendor,
                transaction.Description,
                transaction.Category.Type == "Expense"
                    ? -transaction.AmountInBaseCurrency
                    : transaction.AmountInBaseCurrency))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialSummaryResult>> GetSummary(
        DateTime startDate,
        DateTime endDate,
        string? type,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizeType(type);
        var transactionQuery = context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.TransactionDate >= startDate &&
                transaction.TransactionDate < endDate &&
                transaction.Category != null &&
                (transaction.Category.Type == "Expense" || transaction.Category.Type == "Income"));

        if (normalizedType is not null)
        {
            transactionQuery = transactionQuery.Where(transaction => transaction.Category!.Type == normalizedType);
        }

        var transactionSummary = await transactionQuery
            .GroupBy(transaction => new { transaction.Category!.Type, transaction.Category.Name })
            .Select(group => new FinancialSummaryResult(
                group.Key.Type,
                group.Key.Name,
                group.Key.Type == "Expense"
                    ? -group.Sum(transaction => transaction.AmountInBaseCurrency)
                    : group.Sum(transaction => transaction.AmountInBaseCurrency),
                group.Count()))
            .ToListAsync(cancellationToken);

        if (normalizedType is "Expense")
        {
            return OrderSummary(transactionSummary);
        }

        var invoiceSummary = await context.InvoiceLineItems
            .AsNoTracking()
            .Where(lineItem =>
                lineItem.Invoice.Date >= startDate &&
                lineItem.Invoice.Date < endDate &&
                lineItem.Category != null &&
                lineItem.Category.Type == "Income")
            .GroupBy(lineItem => lineItem.Category!.Name)
            .Select(group => new FinancialSummaryResult(
                "Income",
                group.Key,
                group.Sum(lineItem => lineItem.Quantity * lineItem.UnitAmount),
                group.Count()))
            .ToListAsync(cancellationToken);

        return OrderSummary(transactionSummary
            .Concat(invoiceSummary)
            .GroupBy(item => new { item.Type, item.Category })
            .Select(group => new FinancialSummaryResult(
                group.Key.Type,
                group.Key.Category,
                group.Sum(item => item.AmountInBaseCurrency),
                group.Sum(item => item.EntryCount))));
    }

    public async Task<IReadOnlyList<FinancialCategoryResult>> GetCategories(
        string? type,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizeType(type);
        var query = context.Categories
            .AsNoTracking()
            .Where(category => category.Type == "Expense" || category.Type == "Income");

        if (normalizedType is not null)
        {
            query = query.Where(category => category.Type == normalizedType);
        }

        return await query
            .OrderBy(category => category.Type)
            .ThenBy(category => category.Name)
            .Select(category => new FinancialCategoryResult(category.CategoryId, category.Name, category.Type))
            .ToListAsync(cancellationToken);
    }

    private static string? NormalizeType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return null;
        }

        if (type.Equals("expense", StringComparison.OrdinalIgnoreCase))
        {
            return "Expense";
        }

        if (type.Equals("income", StringComparison.OrdinalIgnoreCase))
        {
            return "Income";
        }

        throw new ArgumentException("Type must be either 'Expense' or 'Income'.", nameof(type));
    }

    private static IReadOnlyList<FinancialSummaryResult> OrderSummary(IEnumerable<FinancialSummaryResult> items) =>
        items
            .OrderBy(item => item.Type)
            .ThenByDescending(item => item.AmountInBaseCurrency)
            .ToList();
}
