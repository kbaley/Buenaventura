using System.Text;
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
    IConfiguration configuration,
    IInvoiceService invoiceService,
    CoronadoDbContext context,
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
            var config = await context.Configurations.SingleOrDefaultAsync(c => c.Name == "InvoiceTemplate")
                .ConfigureAwait(false);
            if (config == null)
            {
                config = new Configuration
                {
                    ConfigurationId = Guid.NewGuid(),
                    Name = "InvoiceTemplate",
                    Value = template
                };
                context.Configurations.Add(config);
            }
            else
            {
                config.Value = template;
            }

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        return Ok(new { Status = "Uploaded successfully" });
    }

    [HttpGet]
    [Route("{invoiceId}/download")]
    public async Task<IActionResult> PdfInvoice([FromRoute] Guid invoiceId)
    {
        var apiKey = configuration.GetValue<string>("Html2PdfRocketKey") ?? string.Empty;

        using var client = new HttpClient();
        var invoice = await context.FindInvoiceEager(invoiceId).ConfigureAwait(false);
        var parms = new Dictionary<string, string>();
        parms.Add("apikey", apiKey);
        parms.Add("value", GetInvoiceHtml(invoice));
        var content = new FormUrlEncodedContent(parms);
        var result = await client.PostAsync("https://api.html2pdfrocket.com/pdf", content);
        if (result.IsSuccessStatusCode)
        {
            var stream = new MemoryStream(await result.Content.ReadAsByteArrayAsync());
            return File(stream, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
        }

        throw new Exception("Error generating PDF");
    }

    private string GetInvoiceHtml(Invoice invoice)
    {
        var template = context.Configurations.SingleOrDefault(c => c.Name == "InvoiceTemplate");
        if (template == null || string.IsNullOrWhiteSpace(template.Value))
        {
            throw new Exception("No invoice template found");
        }

        if (invoice == null) return template.Value;
        var value = template.Value
            .Replace("{{InvoiceNumber}}", invoice.InvoiceNumber)
            .Replace("{{Balance}}", invoice.Balance.ToString("C"))
            .Replace("{{CustomerName}}", invoice.Customer.Name)
            .Replace("{{CustomerAddress}}", StringExtensions
                .GetAddress(invoice.Customer.StreetAddress, invoice.Customer.City, invoice.Customer.Region)
                .Replace("\n", "<br/>"))
            .Replace("{{InvoiceDate}}", invoice.Date.ToString("MMM dd, yyyy"))
            .Replace("{{DueDate}}", invoice.Date.AddDays(30).ToString("MMM dd, yyyy"));
        var lineItemTemplate = value.Substring(value.IndexOf("{{StartInvoiceLineItem}}", StringComparison.Ordinal));
        lineItemTemplate = lineItemTemplate.Substring(0, lineItemTemplate.IndexOf("{{EndInvoiceLineItem}}", StringComparison.Ordinal))
            .Replace("{{StartInvoiceLineItem}}", "");
        var lineItemNumber = 0;
        var lineItemSection = "";
        foreach (var item in invoice.LineItems)
        {
            lineItemNumber++;
            var section = lineItemTemplate
                .Replace("{{ItemNumber}}", lineItemNumber.ToString())
                .Replace("{{Description}}", item.Description)
                .Replace("{{Quantity}}", item.Quantity.ToString("n2"))
                .Replace("{{UnitAmount}}", item.UnitAmount.ToString("n2"))
                .Replace("{{ItemTotal}}", (item.Quantity * item.UnitAmount).ToString("n2"));
            lineItemSection += section;
        }

        var pos1 = value.IndexOf("{{StartInvoiceLineItem}}", StringComparison.Ordinal);
        var pos2 = value.IndexOf("{{EndInvoiceLineItem}}", StringComparison.Ordinal) + "{{EndInvoiceLineItem}}".Length;
        value = value.Remove(pos1, pos2 - pos1).Insert(pos1, lineItemSection);

        return value;
    }
    
    [HttpGet]
    [Route("{invoiceId}/view")]
    public async Task<IActionResult> ViewInvoice([FromRoute] Guid invoiceId)
    {
        var invoice = await context.FindInvoiceEager(invoiceId).ConfigureAwait(false);
        var html = GetInvoiceHtml(invoice);
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(html));
        return new FileStreamResult(ms, "text/html");
    }

}