using AutoMapper;
using Buenaventura.Data;
using Buenaventura.Dtos;
using Buenaventura.Shared;
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
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction([FromRoute] Guid id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var transaction = await transactionRepo.Get(id);
        InvoiceForPosting? invoiceDto = null;
        if (transaction.InvoiceId.HasValue)
        {
            var invoice = await context.Invoices.FindAsync(transaction.InvoiceId.Value);
            if (invoice != null)
            {
                await context.Entry(invoice).Collection(i => i.LineItems).LoadAsync();
                await context.Entry(invoice).Reference(i => i.Customer).LoadAsync();
                invoiceDto = mapper.Map<InvoiceForPosting>(invoice);
            }
        }
        await transactionRepo.Delete(id);

        return Ok(new { transaction, accountBalances = (await context.GetAccountBalances()).ToList(), invoiceDto });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction([FromRoute] Guid id,
        [FromBody] TransactionForDisplay transaction)
    {

        if (id != transaction.TransactionId)
        {
            return BadRequest();
        }

        var originalAmount = (await transactionRepo.Get(transaction.TransactionId)).Amount;
        transaction.SetAmount();
        await transactionRepo.Update(transaction);
        InvoiceForPosting? invoiceDto = null;
        if (transaction.InvoiceId.HasValue)
        {
            var invoice = context.FindInvoiceEager(transaction.InvoiceId.Value);
            invoiceDto = mapper.Map<InvoiceForPosting>(invoice);
        }
        return Ok(new { transaction, 
            originalAmount, 
            accountBalances = (await context.GetAccountBalances()).ToList(), 
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

        var addedTransactions = await transactionRepo.Insert(transaction);
        transactions.AddRange(addedTransactions.Select(mapper.Map<TransactionForDisplay>));
        transactions.ForEach(t => t.SetDebitAndCredit());

        var accountBalances = (await context.GetAccountBalances()).ToList();
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