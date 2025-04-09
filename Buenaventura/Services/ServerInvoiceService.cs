using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerInvoiceService(
    IDbContextFactory<CoronadoDbContext> dbContextFactory
) : IInvoiceService
{
    public async Task<IEnumerable<InvoiceAsCategory>> GetInvoicesForTransactionCategories()
    {
        var context = await dbContextFactory.CreateDbContextAsync();
        var invoices = await context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Balance > 0)
            .ToListAsync();
        return invoices.Select(i => new InvoiceAsCategory
        {
            InvoiceId = i.InvoiceId,
            InvoiceNumber = i.InvoiceNumber,
            CustomerName = i.Customer.Name,
            Balance = i.Balance,
            IsPaidInFull = i.IsPaidInFull,
            CustomerId = i.CustomerId,
            Date = i.Date
        });
    }
}