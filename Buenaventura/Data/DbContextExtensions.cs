using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Data;

public static class DbContextExtensions
{
    public static async Task<decimal> GetCadExchangeRate(this DbSet<Currency> currencies, DateTime? asOf = null)
    {
        asOf ??= DateTime.Now;

        var currency = (await currencies
            .Where(c => c.Symbol == "CAD")
            .ToListAsync())
            .OrderBy(c => Math.Abs((asOf.Value - c.LastRetrieved).TotalMinutes))
            .First();
        return currency.PriceInUsd;
    }

    public static async Task<IEnumerable<AccountIdAndBalance>> GetAccountBalances(this BuenaventuraDbContext context)
    {
        var exchangeRate = await context.Currencies.GetCadExchangeRate();
        return await context.Accounts
            .Select(a => new AccountIdAndBalance
            {
                AccountId = a.AccountId,
                CurrentBalance = a.Transactions.Sum(t => t.Amount),
                CurrentBalanceInUsd = a.Currency == "CAD"
                    ? Math.Round(a.Transactions.Sum(t => t.Amount) / exchangeRate, 2)
                    : a.Transactions.Sum(t => t.Amount)
            }).ToListAsync();
    }

    public static async Task<double> GetAnnualizedIrr(this DbSet<Investment> investments)
    {
        var transactions = await investments
            .SelectMany(i => i.Transactions)
            .OrderBy(t => t.Date)
            .ToListAsync();
        var dividends = await investments
            .SelectMany(i => i.Dividends)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync();
        if (!transactions.Any() && !dividends.Any()) return 0.0;
        var startDate = transactions.First().Date;
        var payments = new List<double>();
        var days = new List<double>();
        foreach (var trx in transactions)
        {
            payments.Add(0 - (Convert.ToDouble(trx.Shares * trx.Price)));
            days.Add((trx.Date - startDate).Days);
        }

        foreach (var dividend in dividends)
        {
            payments.Add(Convert.ToDouble(dividend.Amount));
            days.Add((dividend.TransactionDate - startDate).Days);
        }

        foreach (var investment in investments)
        {
            payments.Add(Convert.ToDouble(investment.GetCurrentValue()));
            days.Add((DateTime.Today - startDate).Days);
        }

        return Irr.CalculateIrr(payments.ToArray(), days.ToArray());
    }

    public static async Task<Category> GetOrCreateCategory(this BuenaventuraDbContext context, string name)
    {
        var category = await context.Categories.SingleOrDefaultAsync(c => c.Name == name);
        if (category != null)
        {
            return category;
        }

        category = new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = name,
            Type = "Expense"
        };
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
        return category;
    }

    public static async Task<Category?> GetOrCreateCategory(this BuenaventuraDbContext context, CategoryModel categoryModel)
    {
        var categories = await context.Categories.ToListAsync();
        if (categoryModel is { Type: CategoryType.REGULAR, CategoryId: not null })
        {
            return categories.Single(c => c.CategoryId == categoryModel.CategoryId);
        }

        if (categoryModel.Type == CategoryType.FREEFORM)
        {
            return await GetOrCreateCategory(context, categoryModel.Name);
        }

        return null;
    }

    public static async Task RemoveByIdAsync<T>(this DbSet<T> items, Guid id) where T : class
    {
        var item = await items.FindAsync(id);
        if (item != null)
        {
            items.Remove(item);
        }
    }

    public static decimal GetPaymentsFor(this DbSet<Transaction> transactions, Guid invoiceId)
    {
        return transactions
            .Where(t => t.InvoiceId == invoiceId)
            .Sum(t => t.Amount);
    }

    public static async Task<Invoice> FindInvoiceEager(this BuenaventuraDbContext context, Guid invoiceId)
    {
        var invoice = await context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Customer)
            .SingleAsync(i => i.InvoiceId == invoiceId);

        return invoice;
    }
}