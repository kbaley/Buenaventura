using AutoMapper;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AccountsController(
    CoronadoDbContext context,
    ITransactionRepository transactionRepo,
    IMapper mapper)
    : ControllerBase
{
    private readonly TransactionParser _transactionParser = new(context);

    // GET: api/Accounts
    [HttpGet]
    public IEnumerable<AccountWithBalance> GetAccounts()
    {
        var exchangeRate = context.Currencies.GetCadExchangeRate();
        var accounts = context.Accounts
            .Select(a => new AccountWithBalance
            {
                AccountId = a.AccountId,
                Name = a.Name,
                Currency = a.Currency,
                Vendor = a.Currency,
                AccountType = a.AccountType,
                MortgagePayment = a.MortgagePayment,
                MortgageType = a.MortgageType,
                DisplayOrder = a.DisplayOrder,
                IsHidden = a.IsHidden,
                CurrentBalance = a.Transactions.Sum(t => t.Amount),
                CurrentBalanceInUsd = a.Currency == "CAD" 
                    ? Math.Round(a.Transactions.Sum(t => t.Amount) / exchangeRate, 2)
                    : a.Transactions.Sum(t => t.Amount),
            });
        return accounts;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAccount([FromRoute] Guid id, [FromBody] Account account)
    {
        context.Entry(account).State = EntityState.Modified;
        await context.SaveChangesAsync().ConfigureAwait(false);

        return Ok(account);
    }

    [HttpPost]
    [Route("[action]")]
    public IActionResult PostQif([FromForm] AccountQifViewModel model)
    {
        var transactions = 
            model.File != null 
            ? _transactionParser.Parse(model.File, model.AccountId, model.FromDate) 
            : _transactionParser.Parse(model.Transactions, model.AccountId, model.FromDate);
        var insertedTransactions = new List<TransactionForDisplay>();
        foreach (var trx in transactions)
        {
            if (string.IsNullOrWhiteSpace(trx.DownloadId))
            {
                transactionRepo.Insert(trx);
                insertedTransactions.Add(trx);
            }
            else
            {
                var existingTrx = context.Transactions.Any(t => t.DownloadId == trx.DownloadId);
                if (!existingTrx) {
                    transactionRepo.Insert(trx);
                    insertedTransactions.Add(trx);
                }
            }
        }
        return CreatedAtAction("PostQif", new { id = model.AccountId }, insertedTransactions);
    }

    [HttpPost]
    public async Task<IActionResult> PostAccount([FromBody] AccountForPosting account)
    {
        var mappedAccount = mapper.Map<Account>(account);
        mappedAccount.AccountId = Guid.NewGuid();
        context.Accounts.Add(mappedAccount);

        var category = context.GetOrCreateCategory("Starting Balance").GetAwaiter().GetResult();
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
        var exchangeRate = context.Currencies.GetCadExchangeRate();
        var accountCurrency = account.Currency;
        transaction.SetAmountInBaseCurrency(accountCurrency, exchangeRate);
        context.Transactions.Add(transaction);

        var model = new AccountWithTransactions
        {
            AccountId = mappedAccount.AccountId,
            Name = mappedAccount.Name,
            Transactions = new List<TransactionForDisplay>([mapper.Map<TransactionForDisplay>(transaction)]),
            CurrentBalance = account.StartingBalance
        };
        await context.SaveChangesAsync().ConfigureAwait(false);

        return CreatedAtAction("PostAccount", new { id = mappedAccount.AccountId }, model);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount([FromRoute] Guid id)
    {
        var account = await context.Accounts.FindAsync(id).ConfigureAwait(false);
        if (account == null)
        {
            return NotFound();
        }
        context.Remove(account);
        await context.SaveChangesAsync().ConfigureAwait(false);
        return Ok(account);
    }
}