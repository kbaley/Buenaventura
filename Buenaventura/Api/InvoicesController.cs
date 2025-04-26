using System.Text;
using AutoMapper;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Dtos;
using Buenaventura.Services;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Invoice = Buenaventura.Domain.Invoice;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class InvoicesController(
    IConfiguration configuration,
    IInvoiceService invoiceService,
    IInvoiceGenerator invoiceGenerator,
    BuenaventuraDbContext context,
    IMapper mapper) : ControllerBase
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

        await context.SaveChangesAsync();
        await context.Entry(invoiceMapped).Reference(i => i.Customer).LoadAsync();
        invoice = mapper.Map<InvoiceForPosting>(invoiceMapped);

        return Ok(invoice);
    }

    [HttpPost]
    public async Task Post([FromBody] InvoiceModel invoiceModel)
    {
        await invoiceService.CreateInvoice(invoiceModel);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }

        context.Invoices.Remove(invoice);
        await context.SaveChangesAsync();
        return Ok(invoice);
    }

    [HttpGet]
    [Route("{invoiceId}/download")]
    public async Task<IActionResult> PdfInvoice([FromRoute] Guid invoiceId)
    {
        var invoice = await context.FindInvoiceEager(invoiceId);
        var invoiceBytes = await invoiceGenerator.GeneratePdf(invoiceId);
        var stream = new MemoryStream(invoiceBytes);
        return File(stream, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
    }

    [HttpGet]
    [Route("{invoiceId}/view")]
    public async Task<IActionResult> ViewInvoice([FromRoute] Guid invoiceId)
    {
        var html = await invoiceGenerator.GenerateHtml(invoiceId);
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(html));
        return new FileStreamResult(ms, "text/html");
    }
    
    [HttpGet]
    [Route("invoicetemplate")]
    public async Task<IActionResult> GetInvoiceTemplate()
    {
        var html = await invoiceService.GetInvoiceTemplate();
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(html));
        return new FileStreamResult(ms, "text/html");
    }
    
    [HttpPost]
    [Route("invoicetemplate")]
    public async Task SaveInvoiceTemplate([FromBody] InvoiceTemplateModel template)
    {
        await invoiceService.SaveInvoiceTemplate(template.Template);
    }

    [HttpPost]
    [Route("{invoiceId}/email")]
    public async Task<IActionResult> EmailInvoice([FromRoute] Guid invoiceId)
    {
        await invoiceService.EmailInvoice(invoiceId);
        return Ok(new { Status = "Email sent successfully" });
    }

}