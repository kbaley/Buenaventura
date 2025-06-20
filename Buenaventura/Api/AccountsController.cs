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

    [HttpGet("{id}/transactions/all")]
    public async Task<TransactionListModel> GetAllTransactions([FromRoute] Guid id, [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        // Get all transactions without pagination for duplicate checking
        return await accountService.GetAllTransactions(id, start, end); 
    }

    [HttpGet("{id}/transactions/duplicates")]
    public async Task<TransactionListModel> GetPotentialDuplicateTransactions([FromRoute] Guid id)
    {
        return await accountService.GetPotentialDuplicateTransactions(id);
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
                         $"{transaction.Debit?.ToString() ?? ""}," +
                         $"{transaction.Credit?.ToString() ?? ""}," +
                         $"{transaction.RunningTotal}\n";
        }

        var fileName = $"{account.Name.Replace(" ", "_")}_transactions_{DateTime.Now:yyyy-MM-dd}.csv";
        return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
    }
    
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
    
    [HttpGet("{id}")]
    public async Task<AccountWithBalance> GetAccount([FromRoute] Guid id)
    {
        return await accountService.GetAccount(id);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount([FromRoute] Guid id)
    {
        var account = await context.Accounts.FindAsync(id);
        if (account == null)
        {
            return NotFound();
        }
        context.Remove(account);
        await context.SaveChangesAsync();
        return Ok(account);
    }
    
    [HttpPost("order")]
    public async Task<IActionResult> SaveAccountOrder([FromBody] List<OrderedAccount> accountOrders)
    {
        await accountService.SaveAccountOrder(accountOrders);
        return Ok();
    }
}