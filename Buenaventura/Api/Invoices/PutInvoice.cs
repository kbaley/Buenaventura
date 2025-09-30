using AutoMapper;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Dtos;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Invoice = Buenaventura.Domain.Invoice;

namespace Buenaventura.Api.Invoices;

public class PutInvoice(BuenaventuraDbContext context, AutoMapper.IMapper mapper) : Endpoint<InvoiceForPosting, InvoiceForPosting>
{
    public override void Configure()
    {
        Put("/api/invoices/{id}");
    }

    public override async Task HandleAsync(InvoiceForPosting req, CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var newBalance = req.GetLineItemTotal() - context.Transactions.GetPaymentsFor(id);
        req.Balance = newBalance;
        var invoiceMapped = mapper.Map<Invoice>(req);
        context.Entry(invoiceMapped).State = EntityState.Modified;

        foreach (var item in req.LineItems)
        {
            var mappedLineItem = mapper.Map<InvoiceLineItem>(item);
            switch (item.Status.ToLower())
            {
                case "deleted":
                    await context.InvoiceLineItems.RemoveByIdAsync(item.InvoiceLineItemId);
                    break;
                case "added":
                    context.InvoiceLineItems.Add(mappedLineItem);
                    break;
                case "updated":
                    context.Entry(mappedLineItem).State = EntityState.Modified;
                    break;
            }
        }

        await context.SaveChangesAsync(ct);
        await context.Entry(invoiceMapped).Reference("Customer").LoadAsync(ct);
        var response = mapper.Map<InvoiceForPosting>(invoiceMapped);

        await SendOkAsync(response, ct);
    }
}
