using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buenaventura.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class MortgagesController(CoronadoDbContext context, ITransactionRepository transactionRepo)
    : ControllerBase
{
    private readonly CoronadoDbContext _context = context;
    private readonly ITransactionRepository _transactionRepo = transactionRepo;

    [HttpPost]
    public IActionResult PostMortgagePayment([FromBody] TransactionForDisplay transaction)
    {

        // This likely doesn't work anymore but I don't have a mortgage so...
        throw new NotImplementedException();

        // var principal = transaction.Debit.Value;
        // var interest = transaction.Credit.Value;
        // transaction.TransactionType = TRANSACTION_TYPE.REGULAR;
        // transaction.Credit = null;
        // var transactions = new List<TransactionForDisplay>();
        // if (transaction.TransactionId == null || transaction.TransactionId == Guid.Empty) transaction.TransactionId = Guid.NewGuid();
        // if (!transaction.AccountId.HasValue) {
        //     transaction.AccountId = _accountRepo.GetAllWithBalances().Single(
        //             a => a.Name.Equals(transaction.AccountName, StringComparison.CurrentCultureIgnoreCase)).AccountId;
        // }
        // transaction.SetAmount();
        // var relatedTransactionId = Guid.NewGuid();
        // var relatedTransaction = new TransactionForDisplay {
        //     TransactionDate = transaction.TransactionDate,
        //     TransactionId = relatedTransactionId,
        //     Vendor = transaction.Vendor,
        //     Description = string.IsNullOrWhiteSpace(transaction.Description) ? "Mortgage Payment" : transaction.Description,
        //     AccountId = transaction.RelatedAccountId.Value,
        //     Amount = 0 - transaction.Amount,
        //     EnteredDate = DateTime.Now,
        //     RelatedTransactionId = transaction.TransactionId
        // };
        // relatedTransaction.SetDebitAndCredit();

        // transaction.RelatedTransactionId = relatedTransactionId;

        // var mortgageInterestCategoryId = (await _context.GetOrCreateCategory("Mortgage Interest").ConfigureAwait(false)).CategoryId;
        // var interestTransaction = new TransactionForDisplay {
        //     TransactionDate = transaction.TransactionDate,
        //     TransactionId = Guid.NewGuid(),
        //     Vendor = transaction.Vendor,
        //     AccountId = transaction.AccountId,
        //     Amount = 0 - interest,
        //     Debit = interest,
        //     EnteredDate = DateTime.Now,
        //     CategoryId = mortgageInterestCategoryId,
        //     CategoryDisplay = "Mortgage Interest"
        // };

        // var bankFeeTransactions = TransactionHelpers.GetBankFeeTransactions(transaction, _context);
        // transactions.Add(transaction);
        // transactions.Add(relatedTransaction);
        // transactions.Add(interestTransaction);
        // transactions.AddRange(bankFeeTransactions);
        // _transactionRepo.InsertRelatedTransaction(transaction, relatedTransaction);
        // _transactionRepo.Insert(interestTransaction);
        // foreach(var trx in bankFeeTransactions) {
        //     _transactionRepo.Insert(trx);
        // }

        // var accountBalances = _context.Accounts.GetAccountBalances();
        // return CreatedAtAction("PostTransfer", 
        //     new { id = transaction.TransactionId }, new {transactions, accountBalances});
    }
}