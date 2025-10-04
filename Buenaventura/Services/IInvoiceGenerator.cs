using Buenaventura.Data;
using Buenaventura.Domain;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IInvoiceGenerator : IAppService
{
    Task<string> GenerateHtml(Guid invoiceId);
    Task<byte[]> GeneratePdf(Guid invoiceId);
}

public class InvoiceGenerator(
    BuenaventuraDbContext context,
    IConfiguration configuration
    ) : IInvoiceGenerator
{
    public async Task<string> GenerateHtml(Guid invoiceId)
    {
        var invoice = await context.FindInvoiceEager(invoiceId);
        var template = await context.Configurations.SingleOrDefaultAsync(c => c.Name == "InvoiceTemplate");
        if (template == null || string.IsNullOrWhiteSpace(template.Value))
        {
            throw new Exception("No invoice template found");
        }

        if (invoice == null) return template.Value;
        var value = template.Value
            .Replace("{{InvoiceNumber}}", invoice.InvoiceNumber)
            .Replace("{{Balance}}", invoice.Balance.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US")))
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

    public async Task<byte[]> GeneratePdf(Guid invoiceId)
    {
        var apiKey = configuration.GetValue<string>("Html2PdfRocketKey") ?? string.Empty;

        using var client = new HttpClient();
        var parms = new Dictionary<string, string>();
        parms.Add("apikey", apiKey);
        parms.Add("value", await GenerateHtml(invoiceId));
        parms.Add("encoding", "UTF-8");
        var content = new FormUrlEncodedContent(parms);
        var result = await client.PostAsync("https://api.html2pdfrocket.com/pdf", content);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadAsByteArrayAsync();
        }

        throw new Exception("Error generating PDF");
    }
}