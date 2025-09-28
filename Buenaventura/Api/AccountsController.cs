using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AccountsController(
    BuenaventuraDbContext context,
    IAccountService accountService)
    : ControllerBase
{
    [HttpPost("{id}/transactions")]
    public async Task CreateTransaction([FromRoute] Guid id, [FromBody] TransactionForDisplay transaction)
    {
        await accountService.AddTransaction(id, transaction);
    }
    
    [HttpPost("{id}/transactions/bulk")]
    public async Task AddBulkTransactions([FromRoute] Guid id, [FromBody] List<TransactionForDisplay> transactions)
    {
        await accountService.AddBulkTransactions(id, transactions);
    }

    [HttpPut("{id}")]
    public async Task PutAccount([FromRoute] Guid id, [FromBody] AccountWithBalance account)
    {
        if (id != account.AccountId)
        {
            throw new Exception("AccountId does not match");
        }
        await accountService.UpdateAccount(account);
    }

    [HttpPost]
    public async Task<IActionResult> PostAccount([FromBody] AccountForPosting account)
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
        await context.SaveChangesAsync();

        return CreatedAtAction("PostAccount", new { id = mappedAccount.AccountId }, model);
    }
}