using AutoMapper;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Dtos;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class InvoicesController(
    IInvoiceService invoiceService,
    CoronadoDbContext context, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<InvoiceModel>> GetInvoices()
    {
        return await invoiceService.GetInvoices();
    }
    
    [HttpGet("nextinvoicenumber")]
    public async Task<int> GetNextInvoiceNumber()
    {
        return await invoiceService.GetNextInvoiceNumber();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] InvoiceForPosting invoice)
    {
        var newBalance = invoice.GetLineItemTotal() - context.Transactions.GetPaymentsFor(id);
        invoice.Balance = newBalance;
        var invoiceMapped = mapper.Map<Invoice>(invoice);
        context.Entry(invoiceMapped).State = EntityState.Modified;

        foreach (var item in invoice.LineItems)
        {
            var mappedLineItem = mapper.Map<InvoiceLineItem>(item);
            switch (item.Status.ToLower()) {
                case "deleted":
                    await context.InvoiceLineItems.RemoveByIdAsync(item.InvoiceLineItemId).ConfigureAwait(false);
                    break;
                case "added":
                    context.InvoiceLineItems.Add(mappedLineItem);
                    break;
                case "updated":
                    context.Entry(mappedLineItem).State = EntityState.Modified;
                    break;
            }
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
        await context.Entry(invoiceMapped).Reference(i => i.Customer).LoadAsync().ConfigureAwait(false);
        invoice = mapper.Map<InvoiceForPosting>(invoiceMapped);

        return Ok(invoice);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] InvoiceForPosting invoice)
    {
        if (invoice.InvoiceId == Guid.Empty) invoice.InvoiceId = Guid.NewGuid();
        invoice.Balance = invoice.GetLineItemTotal();
        var invoiceMapped = mapper.Map<Invoice>(invoice);
        context.Invoices.Add(invoiceMapped);
        await context.SaveChangesAsync();
        var customer = await context.Customers.FindAsync(invoice.CustomerId);
        invoice.CustomerName = customer!.Name;
        invoice.CustomerEmail = customer.Email;

        return CreatedAtAction("Post", new { id = invoice.InvoiceId }, invoice);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice == null) {
            return NotFound();
        }
        context.Invoices.Remove(invoice);
        await context.SaveChangesAsync().ConfigureAwait(false);
        return Ok(invoice);
    }
        
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> UploadTemplate([FromForm] UploadTemplateViewModel model)
    {
        var file = model.File;
        if (file!.Length > 0)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var template = await reader.ReadToEndAsync().ConfigureAwait(false);
            var config = await context.Configurations.SingleOrDefaultAsync(c => c.Name == "InvoiceTemplate").ConfigureAwait(false);
            if (config == null) {
                config = new Configuration {
                    ConfigurationId = Guid.NewGuid(),
                    Name = "InvoiceTemplate",
                    Value = template
                };
                context.Configurations.Add(config);
            } else {
                config.Value = template;
            }
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
        return Ok(new { Status = "Uploaded successfully" } );
    }
}