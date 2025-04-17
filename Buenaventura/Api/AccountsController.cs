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
public class AccountsController(
    BuenaventuraDbContext context,
    ITransactionRepository transactionRepo,
    IAccountService accountService,
    IMapper mapper)
    : ControllerBase
{
    private readonly TransactionParser _transactionParser = new(context);

    [HttpGet]
    public async Task<IEnumerable<AccountWithBalance>> GetAccounts()
    {
        return await accountService.GetAccounts();
    }

    [HttpGet("{id}/transactions")]
    public async Task<TransactionListModel> GetTransactions([FromRoute] Guid id, [FromQuery] UrlQuery query)
    {
        return await accountService.GetTransactions(id, query.Search ?? "", query.Page, query.PageSize);
    }

    [HttpGet("{id}/transactions/csv")]
    public async Task<IActionResult> DownloadTransactionsCsv([FromRoute] Guid id)
    {
        var account = await accountService.GetAccount(id);
        var transactions = await accountService.GetTransactions(id, "", 0, int.MaxValue);
        
        var csvContent = "Date,Vendor,Category,Description,Debit,Credit,Balance\n";
        foreach (var transaction in transactions.Items)
        {
            csvContent += $"{transaction.TransactionDate:MM/dd/yyyy}," +
                         $"\"{transaction.Vendor?.Replace("\"", "\"\"")}\"," +
                         $"\"{transaction.Category.Name.Replace("\"", "\"\"")}\"," +
                         $"\"{transaction.Description?.Replace("\"", "\"\"")}\"," +
                         $"{transaction.Debit?.ToString("N2") ?? ""}," +
                         $"{transaction.Credit?.ToString("N2") ?? ""}," +
                         $"{transaction.RunningTotal:N2}\n";
        }

        var fileName = $"{account.Name.Replace(" ", "_")}_transactions_{DateTime.Now:yyyy-MM-dd}.csv";
        return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
    }
    
    [HttpPost("{id}/transactions")]
    public async Task CreateTransaction([FromRoute] Guid id, [FromBody] TransactionForDisplay transaction)
    {
        await accountService.AddTransaction(id, transaction);
    }
    
    [HttpGet("{id}")]
    public async Task<AccountWithBalance> GetAccount([FromRoute] Guid id)
    {
        return await accountService.GetAccount(id);
    }
    

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAccount([FromRoute] Guid id, [FromBody] Account account)
    {
        context.Entry(account).State = EntityState.Modified;
        await context.SaveChangesAsync().ConfigureAwait(false);

        return Ok(account);
    }

    // [HttpPost]
    // [Route("[action]")]
    // public IActionResult PostQif([FromForm] AccountQifViewModel model)
    // {
    //     var transactions = 
    //         model.File != null 
    //         ? _transactionParser.Parse(model.File, model.AccountId, model.FromDate) 
    //         : _transactionParser.Parse(model.Transactions, model.AccountId, model.FromDate);
    //     var insertedTransactions = new List<TransactionForDisplay>();
    //     foreach (var trx in transactions)
    //     {
    //         if (string.IsNullOrWhiteSpace(trx.DownloadId))
    //         {
    //             transactionRepo.Insert(trx);
    //             insertedTransactions.Add(trx);
    //         }
    //         else
    //         {
    //             var existingTrx = context.Transactions.Any(t => t.DownloadId == trx.DownloadId);
    //             if (!existingTrx) {
    //                 transactionRepo.Insert(trx);
    //                 insertedTransactions.Add(trx);
    //             }
    //         }
    //     }
    //     return CreatedAtAction("PostQif", new { id = model.AccountId }, insertedTransactions);
    // }

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
    
    [HttpPost("order")]
    public async Task<IActionResult> SaveAccountOrder([FromBody] List<OrderedAccount> accountOrders)
    {
        await accountService.SaveAccountOrder(accountOrders);
        return Ok();
    }
}