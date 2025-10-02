using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IAdminService : IServerAppService
{
    Task ScrambleDatabase(ScrambleModel model);
}

public class AdminService(
    BuenaventuraDbContext context
    ): IAdminService
{
    public async Task ScrambleDatabase(ScrambleModel model)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var accountNames = new List<string>
        {
            "Vault of Giggles",
            "Pinball Bank",
            "Bank of Imagination",
            "Piggy Bank of Haiti",
            "Cha-Ching Chase",
            "Pinnacle Bank",
            "Invest This!",
            "Treat Yo' Self Fund",
            "The Nope Account",
            "Dog's College Fund",
            "Subscription Graveyard",
            "Retire at 93 Fund"
        };

        var creditCardNames = new List<string>
        {
            "Buy Now Cry Later Visa",
            "YOLO Platinum",
            "Maxx'd Out Mastercard",
            "Impulse Elite",
            "Debt Vader",
            "Broke AF Black Card",
            "The Enabler Card",
            "Swipe Right",
            "The Forbidden Tap",
            "The Emotional Support Card",
            "Invisible Money Machine",
            "Master of Debt"
        };

        var customerNames = new List<string>
        {
            "Ctrl Alt Elite",
            "Null & Void Inc.",
            "The Cloud Crowd",
            "Runtime Errors & Sons",
            "Bit Happens Consulting",
            "Ping's Anatomy",
            "404 Solutions",
        };

        var firstNames = new List<string>
        {
            "John",
            "Jane",
            "Alex",
            "Chris",
            "Taylor",
            "Jordan",
            "Morgan"
        };
        var emails = new List<string>
        {
            "caffeine.addict@sleeplesscoding.com",
            "error404@brainnotfound.net",
            "ctrl.alt.defeat@keystroke.fail",
            "bug.collector@featurecreep.dev",
            "semicolon.hunter@syntaxpolice.org",
            "password.is.password@securityfail.com",
            "stack.overflown@recursion.hell"
        };

        var assetNames = new List<string>
        {
            "Debt Fortress",
            "Casa de Questional Decisions",
            "Fixer-Upper Fiasco",
            "The Retirement Delayer",
            "House of Snacks",
            "The Accidental AirBnB",
            "Broke Wagon 3000"
        };

        var loanNames = new List<string>
        {
            "The Money Pit",
            "The Never-Ending Loan",
            "LoanlyFans",
            "The Horcrux",
            "Interestzilla"
        };

        var accounts = context.Accounts
            .Where(a => a.AccountType == "Bank Account" && a.IsHidden == false);
        // for each account, take a name from accountNames and rename the account then remove it from the list of account names
        foreach (var account in accounts)
        {
            var randomIndex = Random.Shared.Next(accountNames.Count);
            account.Name = accountNames[randomIndex];
            accountNames.RemoveAt(randomIndex);
        }
        
        // Do the same for credit cards
        var creditCards = context.Accounts
            .Where(a => a.AccountType == "Credit Card" && a.IsHidden == false);
        foreach (var creditCard in creditCards)
        {
            var randomIndex = Random.Shared.Next(creditCardNames.Count);
            creditCard.Name = creditCardNames[randomIndex];
            creditCardNames.RemoveAt(randomIndex);
        }
        
        // And for assets
        var assets = context.Accounts
            .Where(a => a.AccountType == "Asset" && a.IsHidden == false);
        foreach (var asset in assets)
        {
            var randomIndex = Random.Shared.Next(assetNames.Count);
            asset.Name = assetNames[randomIndex];
            assetNames.RemoveAt(randomIndex);
        }
        
        // And for Customers
        var customers = context.Customers;
        foreach (var customer in customers)
        {
            var randomIndex = Random.Shared.Next(customerNames.Count);
            customer.Name = customerNames[randomIndex];
            customer.ContactName = firstNames[randomIndex];
            customer.Email = emails[randomIndex];
            customerNames.RemoveAt(randomIndex);
        }
        
        // And for loans
        var loans = context.Accounts
            .Where(a => a.AccountType == "Loan" && a.IsHidden == false);
        foreach (var loan in loans)
        {
            var randomIndex = Random.Shared.Next(loanNames.Count);
            loan.Name = loanNames[randomIndex];
            loanNames.RemoveAt(randomIndex);
        }
        
        // Delete all transactions before model.DeleteBeforeDate EXCEPT the ones marked as Starting Balance category
        var startingBalanceCategory = await context.Categories
            .FirstOrDefaultAsync(c => c.Name == "Starting Balance");
        var sql = "";
        if (startingBalanceCategory != null)
        {
            sql = $"DELETE FROM transactions WHERE transaction_date < '{model.DeleteBeforeDate:s}' AND category_id != '{startingBalanceCategory.CategoryId}'";
            await context.Database.ExecuteSqlRawAsync(sql);
        }

        // Delete all investments and related transactions
        var investmentTransactions = await context.InvestmentTransactions
            .Include(t => t.Transaction)
            .ToListAsync();
        foreach (var investmentTransaction in investmentTransactions)
        {
            context.Transactions.Remove(investmentTransaction.Transaction);
            context.InvestmentTransactions.Remove(investmentTransaction);
        }
        var dividendTransactions = await context.Transactions
            .Where(t => t.DividendInvestmentId != null)
            .ToListAsync();
        foreach (var dividendTransaction in dividendTransactions)
        {
            context.Transactions.Remove(dividendTransaction);
        }
        var investments = await context.Investments
            .ToListAsync();
        foreach (var investment in investments)
        {
            context.Investments.Remove(investment);
        }
        
        sql = "DELETE FROM Transactions WHERE account_id IN (SELECT account_id FROM accounts WHERE name = 'Investments')";
        await context.Database.ExecuteSqlRawAsync(sql);

        var allCategories = context.Categories.ToList();
        foreach (var category in model.Categories)
        {
            var existingCategory = allCategories.SingleOrDefault(c => c.CategoryId == category.CategoryId);
            if (existingCategory != null && existingCategory.Name != category.Name) {
                existingCategory.Name = category.Name;
            }
        }

        var invoices = await context.Invoices
            .Include(i => i.LineItems)
            .ToListAsync();
        foreach (var invoice in invoices)
        {
            if (invoice.Date < model.DeleteBeforeDate)
            {
                var invoiceTransactions = context.Transactions
                    .Where(t => t.InvoiceId == invoice.InvoiceId);
                foreach (var invoiceTransaction in invoiceTransactions)
                {
                    context.Transactions.Remove(invoiceTransaction);   
                }
                context.InvoiceLineItems.RemoveRange(invoice.LineItems);
                context.Invoices.Remove(invoice);
            }
            else
            {
                foreach (var lineItem in invoice.LineItems)
                {
                    lineItem.UnitAmount *= 0.5m;
                    if (lineItem.Description != null && lineItem.Description.StartsWith("Consulting"))
                    {
                        lineItem.Description = $"Consulting for {invoice.Date:MMMM yyyy}";
                    }
                    context.InvoiceLineItems.Update(lineItem);
                }

                var invoiceTransactions = context.Transactions
                    .Where(t => t.InvoiceId == invoice.InvoiceId);
                if (invoiceTransactions.Count() == 1)
                {
                    invoiceTransactions.First().Amount = invoice.LineItems.Sum(l => l.UnitAmount * l.Quantity);
                }
            }
        }

        await context.SaveChangesAsync();
        
        var bankFeesCategory = await context.Categories.SingleAsync(c => c.Name == "Bank Fees");

        foreach (var remainingTransaction in context.Transactions.Where(t => t.TransactionType == TransactionType.REGULAR))
        {
            // get a random decimal between 0.75 and 1.25
            var randomDecimal = Random.Shared.Next(75, 126) / 100m;
            if (remainingTransaction.CategoryId == bankFeesCategory.CategoryId)
            {
                remainingTransaction.Vendor = "";
            }
            remainingTransaction.Amount *= randomDecimal;
        }
        foreach (var remainingTransaction in context.Transactions.Where(t => t.TransactionType == TransactionType.REGULAR))
        {
            // get a random decimal between 0.75 and 1.25
            var randomDecimal = Random.Shared.Next(75, 126) / 100m;
            remainingTransaction.Amount *= randomDecimal;
        }
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}