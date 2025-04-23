using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;
using Resend;

namespace Buenaventura.Services;

public class ServerInvoiceService(
    BuenaventuraDbContext context,
    IInvoiceGenerator invoiceGenerator,
    IResend resend
) : IInvoiceService
{
    public async Task<IEnumerable<InvoiceModel>> GetInvoices()
    {
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

    public async Task<string> GetInvoiceTemplate()
    {
        var template = (await context.Configurations
            .SingleOrDefaultAsync(c => c.Name == "InvoiceTemplate"))?.Value;
        return template ?? "<html><body>No template found</body></html>";
    }

    public async Task SaveInvoiceTemplate(string template)
    {
        var config = await context.Configurations
            .SingleOrDefaultAsync(c => c.Name == "InvoiceTemplate");
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
        await context.SaveChangesAsync();
        
    }

    public async Task EmailInvoice(Guid invoiceId)
    {
        var invoice = await context.FindInvoiceEager(invoiceId);
        if (invoice == null)
        {
            throw new Exception("Invoice not found");
        }

        var from = new EmailAddress { Email = "kyle@baley.org", DisplayName = "Kyle Baley" };
        var to = new EmailAddress{ Email = invoice.Customer.Email, DisplayName = invoice.Customer.Name};
        var cc = new EmailAddress{ Email = "kyle@baley.org", DisplayName = "Kyle Baley"};
        var subject = $"Invoice {invoice.InvoiceNumber} from Kyle Baley Consulting Ltd.";
        
        var pdfContent = await invoiceGenerator.GeneratePdf(invoiceId);
        
        var msg = new EmailMessage
        {
            From = from,
            Subject = subject,
            TextBody = $"Dear {invoice.Customer.ContactName},\n\nPlease find attached your invoice #{invoice.InvoiceNumber}.\n\nTotal Amount: {invoice.Balance:C}\n\nThank you for your business.\n\nBest regards,\nKyle Baley",
            HtmlBody = $"Dear {invoice.Customer.ContactName},<br/><br/>Please find attached your invoice #{invoice.InvoiceNumber} in the amount of {invoice.Balance:C}<br/><br/>Thank you for your business.<br/><br/>Best regards,<br/>Kyle Baley"
        };
        
        msg.To.Add(to);
        msg.Cc = [cc];
        var attachment = new EmailAttachment
        {
            Content = pdfContent,
            ContentType = "application/pdf",
            Filename = $"Invoice-{invoice.InvoiceNumber}.pdf"
        };
        msg.Attachments = [attachment];

        var response = await resend.EmailSendAsync(msg);
        if (!response.Success)
        {
            throw new Exception("Failed to send email", response.Exception);
        }

        // Update the LastSentToCustomer field
        invoice.LastSentToCustomer = DateTime.UtcNow;
        context.Invoices.Update(invoice);
        await context.SaveChangesAsync();
    }
}