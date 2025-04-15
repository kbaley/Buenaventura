using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
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

    public async Task CreateInvoice(InvoiceModel invoiceModel)
    {
        var context = await dbContextFactory.CreateDbContextAsync();
        if (invoiceModel.InvoiceId == Guid.Empty) invoiceModel.InvoiceId = Guid.NewGuid();
        invoiceModel.Balance = invoiceModel.LineItems.Sum(i => i.UnitPrice * i.Quantity);
        var invoice = new Invoice
        {
            InvoiceId = invoiceModel.InvoiceId,
            InvoiceNumber = invoiceModel.InvoiceNumber,
            Date = invoiceModel.Date,
            CustomerId = invoiceModel.CustomerId,
            Balance = invoiceModel.Balance,
            LineItems = invoiceModel.LineItems.Select(i => new InvoiceLineItem
            {
                InvoiceId = invoiceModel.InvoiceId,
                Description = i.Description,
                UnitAmount = i.UnitPrice,
                Quantity = i.Quantity,
                CategoryId = i.Category?.CategoryId,
                InvoiceLineItemId = Guid.NewGuid(),
                
            }).ToList()
        };
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();
    }
}