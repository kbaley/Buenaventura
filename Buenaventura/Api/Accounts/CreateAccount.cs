using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using FastEndpoints;

namespace Buenaventura.Api;

public class CreateAccount(BuenaventuraDbContext context) : Endpoint<AccountForPosting, AccountWithTransactions>
{
    public override void Configure()
    {
        Post("/api/accounts");
    }

    public override async Task HandleAsync(AccountForPosting account, CancellationToken ct)
    {
        var mappedAccount = new Account
        {
            AccountId = account.AccountId,
            Name = account.Name,
            Currency = account.Currency,
            Vendor = account.Vendor,
            AccountType = account.AccountType,
            MortgagePayment = account.MortgagePayment,
            MortgageType = account.MortgageType,
            IsHidden = account.IsHidden,
            DisplayOrder = account.DisplayOrder
        };
        mappedAccount.AccountId = Guid.NewGuid();
        context.Accounts.Add(mappedAccount);

        var category = await context.GetOrCreateCategory("Starting Balance");
        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = mappedAccount.AccountId,
            Amount = account.StartingBalance,
            AmountInBaseCurrency = account.StartingBalance,
            TransactionDate = account.StartDate,
            Vendor = "",
            Description = "",
            CategoryId = category.CategoryId,
            EnteredDate = account.StartDate,
            IsReconciled = true
        };
        var exchangeRate = await context.Currencies.GetCadExchangeRate();
        var accountCurrency = account.Currency;
        transaction.SetAmountInBaseCurrency(accountCurrency, exchangeRate);
        context.Transactions.Add(transaction);

        var model = new AccountWithTransactions
        {
            AccountId = mappedAccount.AccountId,
            Name = mappedAccount.Name,
            Transactions = new List<TransactionForDisplay>([transaction.ToDto()]),
            CurrentBalance = account.StartingBalance
        };
        await context.SaveChangesAsync(ct);

        await SendOkAsync(model, ct);
    }
}