using AutoMapper;
using Buenaventura.Data;
using Buenaventura.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TransactionsController(
    CoronadoDbContext context,
    ITransactionRepository transactionRepo,
    IMapper mapper)
    : ControllerBase
{
    [HttpGet]
    public TransactionListModel GetTransactions([FromQuery] UrlQuery query)
    {
        if (query.LoadAll) {
            var transactions = transactionRepo.GetByAccount(query.AccountId);
            return new TransactionListModel {
                Transactions = transactions,
                StartingBalance = 0,
                RemainingTransactionCount = 0
            };
        }
        return transactionRepo.GetByAccount(query.AccountId, query.Page);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteTransaction([FromRoute] Guid id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var transaction = transactionRepo.Get(id);
        InvoiceForPosting? invoiceDto = null;
        if (transaction.InvoiceId.HasValue)
        {
            var invoice = context.Invoices.Find(transaction.InvoiceId.Value);
            context.Entry(invoice).Collection(i => i!.LineItems).Load();
            context.Entry(invoice).Reference(i => i!.Customer).Load();
            invoiceDto = mapper.Map<InvoiceForPosting>(invoice);
        }
        transactionRepo.Delete(id);

        return Ok(new { transaction, accountBalances = context.GetAccountBalances().ToList(), invoiceDto });
    }

    [HttpPut("{id}")]
    public IActionResult UpdateTransaction([FromRoute] Guid id,
        [FromBody] TransactionForDisplay transaction)
    {

        if (id != transaction.TransactionId)
        {
            return BadRequest();
        }

        var originalAmount = transactionRepo.Get(transaction.TransactionId).Amount;
        transaction.SetAmount();
        transactionRepo.Update(transaction);
        InvoiceForPosting? invoiceDto = null;
        if (transaction.InvoiceId.HasValue)
        {
            var invoice = context.FindInvoiceEager(transaction.InvoiceId.Value);
            invoiceDto = mapper.Map<InvoiceForPosting>(invoice);
        }
        return Ok(new { transaction, 
            originalAmount, 
            accountBalances = context.GetAccountBalances().ToList(), 
            invoiceDto 
        });
    }

    [HttpPost]
    public async Task<IActionResult> PostTransaction([FromBody] TransactionForDisplay transaction)
    {
        var transactions = new List<TransactionForDisplay>();
        if (transaction.TransactionId == Guid.Empty) transaction.TransactionId = Guid.NewGuid();
        transaction.AccountId ??= context.Accounts
            .Single(a => a.Name.Equals(transaction.AccountName, StringComparison.CurrentCultureIgnoreCase)).AccountId;
        transaction.SetAmount();
        transaction.EnteredDate = DateTime.Now;
        if (transaction.CategoryId.IsNullOrEmpty() && !string.IsNullOrWhiteSpace(transaction.CategoryName))
        {
            transaction.CategoryId = context.GetOrCreateCategory(transaction.CategoryName).GetAwaiter().GetResult().CategoryId;
        }

        var addedTransactions = transactionRepo.Insert(transaction);
        transactions.AddRange(addedTransactions.Select(t => mapper.Map<TransactionForDisplay>(t)));
        transactions.ForEach(t => t.SetDebitAndCredit());

        var accountBalances = context.GetAccountBalances().ToList();
        InvoiceForPosting? invoiceDto = null;
        if (transaction.InvoiceId.HasValue)
        {
            var invoice = await context.FindInvoiceEager(transaction.InvoiceId.Value).ConfigureAwait(false);
            invoiceDto = mapper.Map<InvoiceForPosting>(invoice);
        }
        var vendor = context.Vendors.SingleOrDefault(v => v.Name == transaction.Vendor);

        return CreatedAtAction("PostTransaction", new { id = transaction.TransactionId }, new { transactions, accountBalances, invoiceDto, vendor });
    }

}