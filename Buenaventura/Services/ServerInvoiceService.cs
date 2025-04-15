using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerInvoiceService(
    IDbContextFactory<CoronadoDbContext> dbContextFactory
) : IInvoiceService
{
    public async Task<IEnumerable<InvoiceModel>> GetInvoices()
    {
        var context = await dbContextFactory.CreateDbContextAsync();
        var invoices = await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .ToListAsync();
        return invoices.Select(i => new InvoiceModel
        {
            InvoiceId = i.InvoiceId,
            InvoiceNumber = i.InvoiceNumber,
            CustomerName = i.Customer.Name,
            CustomerEmail = i.Customer.Email,
            Balance = i.Balance,
            CustomerId = i.CustomerId,
            Date = i.Date,
            LastSentToCustomer = i.LastSentToCustomer,
            Total = i.LineItems.Sum(li => li.Amount)
        });
    }

    public Task DeleteInvoice(Guid invoiceId)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetNextInvoiceNumber()
    {
        var context = await dbContextFactory.CreateDbContextAsync();
        var highestInvoiceNumber = (await context.Invoices
                .Select(i => i.InvoiceNumber)
                .ToListAsync())
            .Where(n => int.TryParse(n, out _))
            .Select(n => int.Parse(n))
            .DefaultIfEmpty(100)
            .Max();

        return highestInvoiceNumber + 1;

    }

    public Task CreateInvoice(InvoiceModel invoice)
    {
        throw new NotImplementedException();
    }
}