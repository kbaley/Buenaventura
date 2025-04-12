using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerInvoiceService(
    IDbContextFactory<CoronadoDbContext> dbContextFactory
) : IInvoiceService
{
    public async Task<IEnumerable<InvoiceDto>> GetInvoices()
    {
        var context = await dbContextFactory.CreateDbContextAsync();
        var invoices = await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .Where(i => i.Balance > 0)
            .ToListAsync();
        return invoices.Select(i => new InvoiceDto
        {
            InvoiceId = i.InvoiceId,
            InvoiceNumber = i.InvoiceNumber,
            CustomerName = i.Customer.Name,
            Balance = i.Balance,
            IsPaidInFull = i.IsPaidInFull,
            CustomerId = i.CustomerId,
            Date = i.Date,
            Total = i.LineItems.Sum(li => li.Amount)
        });
    }
}