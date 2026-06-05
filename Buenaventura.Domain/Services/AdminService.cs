using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IAdminService : IAppService
{
    Task ScrambleDatabase(ScrambleModel model);
    Task ResetDemoDatabase();
}

public class AdminService(
    BuenaventuraDbContext context
    ): IAdminService
{
    private const string Expense = "Expense";
    private const string Income = "Income";

    public async Task ResetDemoDatabase()
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        await ClearFinancialData();

        var categories = SeedCategories();
        var accounts = SeedAccounts();
        var investmentCategories = SeedInvestmentCategories();

        SeedCurrencies();
        SeedCustomersAndInvoices(categories, accounts.Checking);
        SeedHouseholdTransactions(categories, accounts);
        SeedInvestments(categories, accounts, investmentCategories);

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

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

    private async Task ClearFinancialData()
    {
        context.ReimbursementMatchTransactions.RemoveRange(await context.ReimbursementMatchTransactions.ToListAsync());
        context.ReimbursementMatches.RemoveRange(await context.ReimbursementMatches.ToListAsync());
        context.ReimbursementSettlements.RemoveRange(await context.ReimbursementSettlements.ToListAsync());
        context.Transfers.RemoveRange(await context.Transfers.ToListAsync());
        context.InvestmentTransactions.RemoveRange(await context.InvestmentTransactions.ToListAsync());
        context.InvoiceLineItems.RemoveRange(await context.InvoiceLineItems.ToListAsync());
        context.Transactions.RemoveRange(await context.Transactions.ToListAsync());
        context.Invoices.RemoveRange(await context.Invoices.ToListAsync());
        context.Investments.RemoveRange(await context.Investments.ToListAsync());
        context.Accounts.RemoveRange(await context.Accounts.ToListAsync());
        context.Categories.RemoveRange(await context.Categories.ToListAsync());
        context.Customers.RemoveRange(await context.Customers.ToListAsync());
        context.Vendors.RemoveRange(await context.Vendors.ToListAsync());
        context.InvestmentCategories.RemoveRange(await context.InvestmentCategories.ToListAsync());
        context.Currencies.RemoveRange(await context.Currencies.ToListAsync());

        await context.SaveChangesAsync();
    }

    private DemoCategories SeedCategories()
    {
        var categories = new DemoCategories(
            StartingBalance: Category("Starting Balance", Income, excludeFromReports: true),
            Salary: Category("Salary", Income),
            Consulting: Category("Consulting", Income),
            Interest: Category("Interest", Income),
            Rent: Category("Rent", Expense, includeInReports: true),
            Groceries: Category("Groceries", Expense, includeInReports: true),
            Restaurants: Category("Restaurants", Expense, includeInReports: true),
            Utilities: Category("Utilities", Expense, includeInReports: true),
            Transportation: Category("Transportation", Expense, includeInReports: true),
            Travel: Category("Travel", Expense, includeInReports: true),
            Entertainment: Category("Entertainment", Expense),
            Insurance: Category("Insurance", Expense),
            Health: Category("Health", Expense),
            Shopping: Category("Shopping", Expense),
            Subscriptions: Category("Subscriptions", Expense),
            BankFees: Category("Bank Fees", Expense, excludeFromReports: true),
            Savings: Category("Savings", Expense, excludeFromReports: true),
            Investments: Category("Investments", Expense, excludeFromReports: true),
            Dividends: Category("Dividends", Income),
            GainLoss: Category("Gain/loss on investments", Income, excludeFromReports: true),
            Reimbursement: Category("Reimbursement", Income)
        );

        context.Categories.AddRange(categories.All);
        return categories;
    }

    private DemoAccounts SeedAccounts()
    {
        var accounts = new DemoAccounts(
            Checking: Account("Everyday Checking", AccountType.BANK_ACCOUNT, "USD", 1),
            Savings: Account("High-Yield Savings", AccountType.BANK_ACCOUNT, "USD", 2),
            CreditCard: Account("Travel Rewards Visa", AccountType.CREDIT_CARD, "USD", 3),
            Investments: Account("Retirement Portfolio", AccountType.INVESTMENT, "USD", 4),
            Home: Account("Townhouse", AccountType.ASSET, "USD", 5),
            Mortgage: Account("Mortgage", AccountType.MORTGAGE, "USD", 6, mortgagePayment: 2150m, mortgageType: "Fixed"),
            CarLoan: Account("Car Loan", AccountType.LOAN, "USD", 7)
        );

        context.Accounts.AddRange(accounts.All);
        return accounts;
    }

    private List<InvestmentCategory> SeedInvestmentCategories()
    {
        var categories = new List<InvestmentCategory>
        {
            new() { InvestmentCategoryId = Guid.NewGuid(), Name = "US Equity", Percentage = 50m },
            new() { InvestmentCategoryId = Guid.NewGuid(), Name = "International Equity", Percentage = 25m },
            new() { InvestmentCategoryId = Guid.NewGuid(), Name = "Bonds", Percentage = 20m },
            new() { InvestmentCategoryId = Guid.NewGuid(), Name = "Cash", Percentage = 5m },
        };

        context.InvestmentCategories.AddRange(categories);
        return categories;
    }

    private void SeedCurrencies()
    {
        var today = DateTime.Today;
        context.Currencies.AddRange(
            new Currency { CurrencyId = Guid.NewGuid(), Symbol = "USD", LastRetrieved = today, PriceInUsd = 1m },
            new Currency { CurrencyId = Guid.NewGuid(), Symbol = "CAD", LastRetrieved = today, PriceInUsd = 1.36m }
        );
    }

    private void SeedCustomersAndInvoices(DemoCategories categories, Account checking)
    {
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = "Northstar Analytics",
            ContactName = "Casey",
            StreetAddress = "100 Demo Plaza",
            City = "Portland",
            Region = "OR",
            Email = "casey@northstar.example"
        };

        context.Customers.Add(customer);

        var invoiceMonths = DemoMonths().Where((_, index) => index % 3 == 1).TakeLast(6).ToList();
        for (var index = 0; index < invoiceMonths.Count; index++)
        {
            var month = invoiceMonths[index];
            var invoice = new Invoice
            {
                InvoiceId = Guid.NewGuid(),
                InvoiceNumber = $"DEMO-{month:yyyyMM}",
                Customer = customer,
                CustomerId = customer.CustomerId,
                Date = month.AddDays(10),
                Balance = 0m,
                IsPaidInFull = true,
                LastSentToCustomer = month.AddDays(11)
            };

            var amount = 950m + (index % 3 * 175m);
            var lineItem = new InvoiceLineItem
            {
                InvoiceLineItemId = Guid.NewGuid(),
                Invoice = invoice,
                InvoiceId = invoice.InvoiceId,
                Category = categories.Consulting,
                CategoryId = categories.Consulting.CategoryId,
                Description = $"Consulting for {month:MMMM yyyy}",
                Quantity = 1m,
                UnitAmount = amount
            };

            var payment = RegularTransaction(
                checking,
                categories.Consulting,
                "Northstar Analytics",
                $"Invoice {invoice.InvoiceNumber} payment",
                amount,
                month.AddDays(24),
                TransactionType.INVOICE_PAYMENT);
            payment.Invoice = invoice;
            payment.InvoiceId = invoice.InvoiceId;

            context.Invoices.Add(invoice);
            context.InvoiceLineItems.Add(lineItem);
            context.Transactions.Add(payment);
        }
    }

    private void SeedHouseholdTransactions(DemoCategories categories, DemoAccounts accounts)
    {
        var months = DemoMonths();

        AddStartingBalance(accounts.Checking, categories.StartingBalance, 4200m, months.First().AddDays(-1));
        AddStartingBalance(accounts.Savings, categories.StartingBalance, 18000m, months.First().AddDays(-1));
        AddStartingBalance(accounts.CreditCard, categories.StartingBalance, -740m, months.First().AddDays(-1));
        AddStartingBalance(accounts.Investments, categories.StartingBalance, 54500m, months.First().AddDays(-1));
        AddStartingBalance(accounts.Home, categories.StartingBalance, 385000m, months.First().AddDays(-1));
        AddStartingBalance(accounts.Mortgage, categories.StartingBalance, -302000m, months.First().AddDays(-1));
        AddStartingBalance(accounts.CarLoan, categories.StartingBalance, -12600m, months.First().AddDays(-1));

        foreach (var month in months)
        {
            var monthIndex = ((month.Year - months.First().Year) * 12) + month.Month - months.First().Month;
            var seasonalBump = month.Month is 11 or 12 ? 175m : 0m;
            var travelMonth = month.Month is 3 or 8;

            AddRegular(accounts.Checking, categories.Salary, "Acme Software", "Payroll deposit", 3850m, month.AddDays(1));
            AddRegular(accounts.Checking, categories.Salary, "Acme Software", "Payroll deposit", 3850m, month.AddDays(15));
            AddRegular(accounts.Checking, categories.Interest, "High-Yield Savings", "Monthly interest", 34m + monthIndex, month.LastDayOfMonth());

            AddRegular(accounts.Checking, categories.Rent, "Evergreen Properties", "Rent", -2250m, month.AddDays(2));
            AddRegular(accounts.Checking, categories.Utilities, "City Utilities", "Electric and water", -185m - (month.Month is 1 or 7 ? 35m : 0m), month.AddDays(6));
            AddRegular(accounts.Checking, categories.Insurance, "Harbor Insurance", "Auto and renter policies", -242m, month.AddDays(9));
            AddRegular(accounts.Checking, categories.Subscriptions, "Streambox", "Streaming bundle", -54m, month.AddDays(12));
            AddRegular(accounts.Checking, categories.BankFees, "Community Bank", "Account maintenance fee", -8m, month.AddDays(28));

            AddRegular(accounts.CreditCard, categories.Groceries, "Green Basket Market", "Groceries", -640m - seasonalBump, month.AddDays(5));
            AddRegular(accounts.CreditCard, categories.Groceries, "Neighborhood Foods", "Groceries", -215m, month.AddDays(19));
            AddRegular(accounts.CreditCard, categories.Restaurants, "Cedar Cafe", "Dining out", -260m - (month.Month == 12 ? 90m : 0m), month.AddDays(13));
            AddRegular(accounts.CreditCard, categories.Transportation, "Metro Fuel", "Fuel and transit", -195m, month.AddDays(17));
            AddRegular(accounts.CreditCard, categories.Entertainment, "Cinema House", "Movies and events", -115m, month.AddDays(22));
            AddRegular(accounts.CreditCard, categories.Health, "Wellness Pharmacy", "Prescriptions", -68m, month.AddDays(24));
            AddRegular(accounts.CreditCard, categories.Shopping, "Home Goods Co.", "Household purchases", -180m - seasonalBump, month.AddDays(26));

            if (travelMonth)
            {
                AddRegular(accounts.CreditCard, categories.Travel, "Summit Air", "Flights", -780m, month.AddDays(8));
                AddRegular(accounts.CreditCard, categories.Travel, "Harbor Hotel", "Hotel stay", -620m, month.AddDays(11));
            }

            AddTransfer(accounts.Checking, accounts.Savings, 700m, month.AddDays(4));
            AddTransfer(accounts.Checking, accounts.CreditCard, 2150m + (travelMonth ? 900m : 0m) + seasonalBump, month.AddDays(27));

            AddRegular(accounts.Mortgage, categories.Rent, "Principal payment", "Mortgage principal", 720m + (monthIndex * 7m), month.AddDays(2), TransactionType.MORTGAGE_PAYMENT);
            AddRegular(accounts.CarLoan, categories.Transportation, "Auto lender", "Principal payment", 410m, month.AddDays(14));
        }
    }

    private void SeedInvestments(DemoCategories categories, DemoAccounts accounts, List<InvestmentCategory> investmentCategories)
    {
        var stockCategory = investmentCategories.Single(c => c.Name == "US Equity");
        var internationalCategory = investmentCategories.Single(c => c.Name == "International Equity");
        var bondCategory = investmentCategories.Single(c => c.Name == "Bonds");

        var investments = new List<Investment>
        {
            Investment("Vanguard Total Market ETF", "VTI", 242.18m, null, paysDividends: true),
            Investment("International Equity ETF", "VXUS", 63.42m, null, paysDividends: true),
            Investment("Total Bond Market ETF", "BND", 74.88m, null, paysDividends: true),
        };

        context.Investments.AddRange(investments);

        var months = DemoMonths();
        foreach (var month in months)
        {
            AddInvestmentPurchase(accounts.Checking, accounts.Investments, categories.Investments, investments[0], 1.45m, 198m + (months.IndexOf(month) * 1.35m), month.AddDays(18));
            AddInvestmentPurchase(accounts.Checking, accounts.Investments, categories.Investments, investments[1], 2.10m, 54m + (months.IndexOf(month) * 0.22m), month.AddDays(18));
            AddInvestmentPurchase(accounts.Checking, accounts.Investments, categories.Investments, investments[2], 0.95m, 71m + (months.IndexOf(month) * 0.08m), month.AddDays(18));

            if (month.Month is 3 or 6 or 9 or 12)
            {
                foreach (var investment in investments)
                {
                    AddRegular(
                        accounts.Investments,
                        categories.Dividends,
                        investment.Symbol,
                        $"{investment.Symbol} dividend",
                        investment.Symbol == "VTI" ? 96m : investment.Symbol == "VXUS" ? 58m : 42m,
                        month.AddDays(25),
                        TransactionType.DIVIDEND,
                        dividendInvestmentId: investment.InvestmentId);
                }
            }

            var gainLoss = month.Month % 4 == 0 ? -430m : 280m + (month.Month * 8m);
            AddRegular(accounts.Investments, categories.GainLoss, "Market movement", "Monthly valuation adjustment", gainLoss, month.LastDayOfMonth());
        }
    }

    private void AddStartingBalance(Account account, Category category, decimal amount, DateTime date)
    {
        AddRegular(account, category, "Opening Balance", "Demo starting balance", amount, date);
    }

    private void AddRegular(
        Account account,
        Category category,
        string vendor,
        string description,
        decimal amount,
        DateTime date,
        TransactionType transactionType = TransactionType.REGULAR,
        Guid? dividendInvestmentId = null)
    {
        context.Transactions.Add(RegularTransaction(account, category, vendor, description, amount, date, transactionType, dividendInvestmentId));
    }

    private static Transaction RegularTransaction(
        Account account,
        Category category,
        string vendor,
        string description,
        decimal amount,
        DateTime date,
        TransactionType transactionType = TransactionType.REGULAR,
        Guid? dividendInvestmentId = null)
    {
        return new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Account = account,
            AccountId = account.AccountId,
            Category = category,
            CategoryId = category.CategoryId,
            Vendor = vendor,
            Description = description,
            Amount = amount,
            AmountInBaseCurrency = amount,
            TransactionDate = date,
            EnteredDate = date,
            IsReconciled = date < DateTime.Today.AddDays(-14),
            TransactionType = transactionType,
            DividendInvestmentId = dividendInvestmentId
        };
    }

    private void AddTransfer(Account from, Account to, decimal amount, DateTime date)
    {
        var left = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Account = from,
            AccountId = from.AccountId,
            Vendor = to.Name,
            Description = $"Transfer to {to.Name}",
            Amount = -amount,
            AmountInBaseCurrency = -amount,
            TransactionDate = date,
            EnteredDate = date,
            IsReconciled = date < DateTime.Today.AddDays(-14),
            TransactionType = TransactionType.TRANSFER
        };
        var right = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Account = to,
            AccountId = to.AccountId,
            Vendor = from.Name,
            Description = $"Transfer from {from.Name}",
            Amount = amount,
            AmountInBaseCurrency = amount,
            TransactionDate = date,
            EnteredDate = date,
            IsReconciled = date < DateTime.Today.AddDays(-14),
            TransactionType = TransactionType.TRANSFER
        };
        var transfer = new Transfer
        {
            TransferId = Guid.NewGuid(),
            LeftTransaction = left,
            LeftTransactionId = left.TransactionId,
            RightTransaction = right,
            RightTransactionId = right.TransactionId
        };

        context.Transactions.AddRange(left, right);
        context.Transfers.Add(transfer);
    }

    private void AddInvestmentPurchase(
        Account fundingAccount,
        Account investmentAccount,
        Category category,
        Investment investment,
        decimal shares,
        decimal price,
        DateTime date)
    {
        var amount = Math.Round(shares * price, 2);
        var cashTransaction = RegularTransaction(
            fundingAccount,
            category,
            investment.Symbol,
            $"Buy {investment.Symbol}",
            -amount,
            date,
            TransactionType.INVESTMENT);
        var investmentAccountTransaction = RegularTransaction(
            investmentAccount,
            category,
            investment.Symbol,
            $"Buy {investment.Symbol}",
            amount,
            date,
            TransactionType.INVESTMENT);
        var investmentTransaction = new InvestmentTransaction
        {
            InvestmentTransactionId = Guid.NewGuid(),
            InvestmentId = investment.InvestmentId,
            Shares = shares,
            Price = price,
            Date = date,
            Transaction = cashTransaction,
            TransactionId = cashTransaction.TransactionId
        };
        var transfer = new Transfer
        {
            TransferId = Guid.NewGuid(),
            LeftTransaction = cashTransaction,
            LeftTransactionId = cashTransaction.TransactionId,
            RightTransaction = investmentAccountTransaction,
            RightTransactionId = investmentAccountTransaction.TransactionId
        };

        context.Transactions.AddRange(cashTransaction, investmentAccountTransaction);
        context.InvestmentTransactions.Add(investmentTransaction);
        context.Transfers.Add(transfer);
    }

    private static Category Category(string name, string type, bool includeInReports = false, bool excludeFromReports = false)
    {
        return new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = name,
            Type = type,
            IncludeInReports = includeInReports,
            ExcludeFromTransactionReport = excludeFromReports
        };
    }

    private static Account Account(
        string name,
        string type,
        string currency,
        int displayOrder,
        decimal? mortgagePayment = null,
        string mortgageType = "")
    {
        return new Account
        {
            AccountId = Guid.NewGuid(),
            Name = name,
            AccountType = type,
            Currency = currency,
            Vendor = "",
            DisplayOrder = displayOrder,
            IsHidden = false,
            ExcludeFromReports = false,
            MortgagePayment = mortgagePayment,
            MortgageType = mortgageType
        };
    }

    private static Investment Investment(
        string name,
        string symbol,
        decimal lastPrice,
        InvestmentCategory? category,
        bool paysDividends)
    {
        return new Investment
        {
            InvestmentId = Guid.NewGuid(),
            Name = name,
            Symbol = symbol,
            Currency = "USD",
            LastPrice = lastPrice,
            LastPriceRetrievalDate = DateTime.Today.AddDays(-1),
            Category = category,
            CategoryId = category?.InvestmentCategoryId,
            DontRetrievePrices = false,
            PaysDividends = paysDividends,
            Transactions = new List<InvestmentTransaction>(),
            Dividends = new List<Transaction>()
        };
    }

    private static List<DateTime> DemoMonths()
    {
        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        return Enumerable.Range(0, 24)
            .Select(offset => currentMonth.AddMonths(-23 + offset))
            .ToList();
    }

    private sealed record DemoCategories(
        Category StartingBalance,
        Category Salary,
        Category Consulting,
        Category Interest,
        Category Rent,
        Category Groceries,
        Category Restaurants,
        Category Utilities,
        Category Transportation,
        Category Travel,
        Category Entertainment,
        Category Insurance,
        Category Health,
        Category Shopping,
        Category Subscriptions,
        Category BankFees,
        Category Savings,
        Category Investments,
        Category Dividends,
        Category GainLoss,
        Category Reimbursement)
    {
        public IEnumerable<Category> All =>
        [
            StartingBalance,
            Salary,
            Consulting,
            Interest,
            Rent,
            Groceries,
            Restaurants,
            Utilities,
            Transportation,
            Travel,
            Entertainment,
            Insurance,
            Health,
            Shopping,
            Subscriptions,
            BankFees,
            Savings,
            Investments,
            Dividends,
            GainLoss,
            Reimbursement
        ];
    }

    private sealed record DemoAccounts(
        Account Checking,
        Account Savings,
        Account CreditCard,
        Account Investments,
        Account Home,
        Account Mortgage,
        Account CarLoan)
    {
        public IEnumerable<Account> All =>
        [
            Checking,
            Savings,
            CreditCard,
            Investments,
            Home,
            Mortgage,
            CarLoan
        ];
    }
}
