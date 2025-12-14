using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;
using Resend;
using System.Globalization;

namespace Buenaventura.Services;

public interface IInvoiceService : IAppService
{
    Task<IEnumerable<InvoiceModel>> GetInvoices();
    Task<int> GetNextInvoiceNumber();
    Task CreateInvoice(InvoiceModel invoice);
    Task<string> GetInvoiceTemplate();
    Task SaveInvoiceTemplate(string template);
    Task EmailInvoice(Guid invoiceId);
    Task<InvoiceModel> GetInvoice(Guid invoiceId);
    Task UpdateInvoice(InvoiceModel invoiceModel);
}

public class InvoiceService(
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
        };
        
        // Add line items with proper navigation property
        invoice.LineItems = invoiceModel.LineItems.Select(i => new InvoiceLineItem
        {
            InvoiceLineItemId = Guid.NewGuid(),
            Invoice = invoice,
            Description = i.Description,
            UnitAmount = i.UnitPrice,
            Quantity = i.Quantity,
            CategoryId = i.Category?.CategoryId,
        }).ToList();
        
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
        
        var culture = new CultureInfo("en-US"); 

        var msg = new EmailMessage
        {
            From = from,
            Subject = subject,
            TextBody = $"Hi {invoice.Customer.ContactName},\n\nPlease find attached my invoice #{invoice.InvoiceNumber}.\n\nTotal Amount: {invoice.Balance.ToString("C", culture)}\n\nThank you for your business.\n\nBest regards,\nKyle Baley",
            HtmlBody = $"Hi {invoice.Customer.ContactName},<br/><br/>Please find attached my invoice #{invoice.InvoiceNumber} in the amount of {invoice.Balance.ToString("C", culture)}<br/><br/>Thank you for your business.<br/><br/>Best regards,<br/>Kyle Baley"
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

    public async Task<InvoiceModel> GetInvoice(Guid invoiceId)
    {
        var invoice = await context.FindInvoiceEager(invoiceId);
        if (invoice == null)
        {
            throw new Exception("Invoice not found");
        }

        return new InvoiceModel
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerName = invoice.Customer.Name,
            CustomerEmail = invoice.Customer.Email,
            Balance = invoice.Balance,
            CustomerId = invoice.CustomerId,
            Date = invoice.Date,
            LastSentToCustomer = invoice.LastSentToCustomer,
            Total = invoice.LineItems.Sum(li => li.Amount),
            LineItems = invoice.LineItems.Select(i => new InvoiceLineItemModel
            {
                Description = i.Description,
                UnitPrice = i.UnitAmount,
                Quantity = i.Quantity,
                Category = i.CategoryId != null ? new CategoryModel
                {
                    CategoryId = i.Category?.CategoryId,
                    Name = i.Category?.Name ?? ""
                } : null
            }).ToList()
        };
    }

    public async Task UpdateInvoice(InvoiceModel invoiceModel)
    {
        var invoice = await context.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceModel.InvoiceId);
        if (invoice == null)
        {
            throw new Exception("Invoice not found");
        }
        invoice.InvoiceNumber = invoiceModel.InvoiceNumber;
        invoice.Date = invoiceModel.Date;
        invoice.CustomerId = invoiceModel.CustomerId;
        var amountPaid = invoice.LineItems.Sum(li => li.UnitAmount * li.Quantity) - invoice.Balance;
        var balance = 0m;
        
        // Clear existing line items properly
        invoice.LineItems.Clear();

        // Add new line items from the model
        foreach (var i in invoiceModel.LineItems)
        {
            var lineItem = new InvoiceLineItem
            {
                InvoiceLineItemId = Guid.NewGuid(),
                InvoiceId = invoice.InvoiceId,
                Invoice = invoice,
                Description = i.Description,
                UnitAmount = i.UnitPrice,
                Quantity = i.Quantity,
                CategoryId = i.Category?.CategoryId
            };
            context.InvoiceLineItems.Add(lineItem);
            balance += lineItem.UnitAmount * lineItem.Quantity;
        }

        invoice.Balance = balance - amountPaid;
        context.Invoices.Update(invoice);
        await context.SaveChangesAsync();
    }
}