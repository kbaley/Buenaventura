using Bogus;
using Buenaventura.Domain;
using Buenaventura.Shared;

namespace Buenaventura.Tests.Helpers;

public static class TestDataFactory
{
    public static Faker<Account> AccountFaker => new Faker<Account>()
        .RuleFor(a => a.AccountId, f => f.Random.Guid())
        .RuleFor(a => a.Name, f => f.Finance.AccountName())
        .RuleFor(a => a.Currency, f => f.PickRandom("USD", "CAD", "EUR"))
        .RuleFor(a => a.Vendor, f => f.Company.CompanyName())
        .RuleFor(a => a.AccountType, f => f.PickRandom("Bank Account", "Credit Card", "Asset", "Loan"))
        .RuleFor(a => a.IsHidden, f => f.Random.Bool())
        .RuleFor(a => a.DisplayOrder, f => f.Random.Int(0, 100))
        .RuleFor(a => a.CurrentBalance, f => f.Random.Decimal(-10000, 50000))
        .RuleFor(a => a.MortgagePayment, f => f.Random.Bool() ? f.Random.Decimal(500, 3000) : null)
        .RuleFor(a => a.MortgageType, f => f.PickRandom("Fixed", "Variable", "ARM"));

    public static Faker<Category> CategoryFaker => new Faker<Category>()
        .RuleFor(c => c.CategoryId, f => f.Random.Guid())
        .RuleFor(c => c.Name, f => f.Commerce.Department())
        .RuleFor(c => c.Type, f => f.PickRandom("Expense", "Income", "Transfer"))
        .RuleFor(c => c.IncludeInReports, f => f.Random.Bool())
        .RuleFor(c => c.ParentCategoryId, f => f.Random.Bool() ? f.Random.Guid() : null);

    public static Faker<Customer> CustomerFaker => new Faker<Customer>()
        .RuleFor(c => c.CustomerId, f => f.Random.Guid())
        .RuleFor(c => c.Name, f => f.Company.CompanyName())
        .RuleFor(c => c.Email, f => f.Internet.Email())
        .RuleFor(c => c.ContactName, f => f.Name.FullName())
        .RuleFor(c => c.Address, f => f.Address.FullAddress())
        .RuleFor(c => c.City, f => f.Address.City())
        .RuleFor(c => c.StreetAddress, f => f.Address.StreetAddress())
        .RuleFor(c => c.Region, f => f.Address.State());

    public static Faker<Investment> InvestmentFaker => new Faker<Investment>()
        .RuleFor(i => i.InvestmentId, f => f.Random.Guid())
        .RuleFor(i => i.Name, f => f.Company.CompanyName())
        .RuleFor(i => i.Symbol, f => f.Random.AlphaNumeric(4).ToUpper())
        .RuleFor(i => i.Currency, f => f.PickRandom("USD", "CAD"))
        .RuleFor(i => i.DontRetrievePrices, f => f.Random.Bool())
        .RuleFor(i => i.LastPrice, f => f.Random.Decimal(10, 1000))
        .RuleFor(i => i.LastPriceRetrievalDate, f => f.Date.Recent(30))
        .RuleFor(i => i.CategoryId, f => f.Random.Guid())
        .RuleFor(i => i.PaysDividends, f => f.Random.Bool());

    public static Faker<Transaction> TransactionFaker => new Faker<Transaction>()
        .RuleFor(t => t.TransactionId, f => f.Random.Guid())
        .RuleFor(t => t.AccountId, f => f.Random.Guid())
        .RuleFor(t => t.Vendor, f => f.Company.CompanyName())
        .RuleFor(t => t.Description, f => f.Lorem.Sentence())
        .RuleFor(t => t.Amount, f => f.Random.Decimal(-1000, 1000))
        .RuleFor(t => t.AmountInBaseCurrency, f => f.Random.Decimal(-1000, 1000))
        .RuleFor(t => t.IsReconciled, f => f.Random.Bool())
        .RuleFor(t => t.TransactionDate, f => f.Date.Recent(365))
        .RuleFor(t => t.CategoryId, f => f.Random.Guid())
        .RuleFor(t => t.EnteredDate, f => f.Date.Recent(30))
        .RuleFor(t => t.TransactionType, f => f.PickRandom<TransactionType>())
        .RuleFor(t => t.DownloadId, f => f.Random.Bool() ? f.Random.AlphaNumeric(10) : null);

    public static Faker<TransactionForDisplay> TransactionForDisplayFaker => new Faker<TransactionForDisplay>()
        .RuleFor(t => t.TransactionId, f => f.Random.Guid())
        .RuleFor(t => t.AccountId, f => f.Random.Guid())
        .RuleFor(t => t.Vendor, f => f.Company.CompanyName())
        .RuleFor(t => t.Description, f => f.Lorem.Sentence())
        .RuleFor(t => t.Amount, f => f.Random.Decimal(-1000, 1000))
        .RuleFor(t => t.TransactionDate, f => f.Date.Recent(365))
        .RuleFor(t => t.IsReconciled, f => f.Random.Bool())
        .RuleFor(t => t.TransactionType, f => f.PickRandom<TransactionType>())
        .RuleFor(t => t.Debit, (_, t) => t.Amount > 0 ? t.Amount : null)
        .RuleFor(t => t.Credit, (_, t) => t.Amount < 0 ? Math.Abs(t.Amount) : null)
        .RuleFor(t => t.Category, f => new CategoryModel 
        { 
            CategoryId = f.Random.Guid(), 
            Name = f.Commerce.Department(),
            CategoryClass = f.PickRandom("Expense", "Income", "Transfer")
        });

    public static Faker<InvestmentTransaction> InvestmentTransactionFaker => new Faker<InvestmentTransaction>()
        .RuleFor(it => it.InvestmentTransactionId, f => f.Random.Guid())
        .RuleFor(it => it.InvestmentId, f => f.Random.Guid())
        .RuleFor(it => it.TransactionId, f => f.Random.Guid())
        .RuleFor(it => it.Shares, f => f.Random.Decimal(1, 1000))
        .RuleFor(it => it.Price, f => f.Random.Decimal(10, 500))
        .RuleFor(it => it.Date, f => f.Date.Recent(365));

    public static Faker<CategoryModel> CategoryModelFaker => new Faker<CategoryModel>()
        .RuleFor(c => c.CategoryId, f => f.Random.Guid())
        .RuleFor(c => c.Name, f => f.Commerce.Department())
        .RuleFor(c => c.CategoryClass, f => f.PickRandom("Expense", "Income", "Transfer"))
        .RuleFor(c => c.IncludeInReports, f => f.Random.Bool())
        .RuleFor(c => c.ExcludeFromTransactionReports, f => f.Random.Bool())
        .RuleFor(c => c.TimesUsed, f => f.Random.Int(0, 100));

    public static Faker<CustomerModel> CustomerModelFaker => new Faker<CustomerModel>()
        .RuleFor(c => c.CustomerId, f => f.Random.Guid())
        .RuleFor(c => c.Name, f => f.Company.CompanyName())
        .RuleFor(c => c.Email, f => f.Internet.Email())
        .RuleFor(c => c.ContactName, f => f.Name.FullName())
        .RuleFor(c => c.Address, f => f.Address.FullAddress())
        .RuleFor(c => c.City, f => f.Address.City())
        .RuleFor(c => c.StreetAddress, f => f.Address.StreetAddress())
        .RuleFor(c => c.Region, f => f.Address.State());

    public static Faker<InvestmentModel> InvestmentModelFaker => new Faker<InvestmentModel>()
        .RuleFor(i => i.InvestmentId, f => f.Random.Guid())
        .RuleFor(i => i.Name, f => f.Company.CompanyName())
        .RuleFor(i => i.Symbol, f => f.Random.AlphaNumeric(4).ToUpper())
        .RuleFor(i => i.Currency, f => f.PickRandom("USD", "CAD"))
        .RuleFor(i => i.Shares, f => f.Random.Decimal(1, 1000))
        .RuleFor(i => i.AveragePrice, f => f.Random.Decimal(10, 500))
        .RuleFor(i => i.BookValue, f => f.Random.Decimal(1000, 50000))
        .RuleFor(i => i.CurrentValue, f => f.Random.Decimal(1000, 50000))
        .RuleFor(i => i.LastPrice, f => f.Random.Decimal(10, 1000))
        .RuleFor(i => i.DontRetrievePrices, f => f.Random.Bool())
        .RuleFor(i => i.PaysDividends, f => f.Random.Bool());
}