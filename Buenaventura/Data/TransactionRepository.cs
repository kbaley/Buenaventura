using AutoMapper;
using Buenaventura.Domain;
using Buenaventura.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Data
{
    public class TransactionRepository : ITransactionRepository
    {
        private const int PAGE_SIZE = 100;
        private readonly CoronadoDbContext _context;
        private readonly IMapper _mapper;
        private decimal _cadExchangeRate = decimal.MinValue;

        public TransactionRepository(CoronadoDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private void UpdateInvoiceBalance(Guid? invoiceId, Guid? transactionToIgnore = null)
        {
            if (!invoiceId.HasValue) return;

            // Load all invoice transactions into the cache so that we can work locally.
            // This is because we might have added or updated a related transaction that
            // hasn't been committed to the database yet.
            _context.Transactions
                .Where(t => t.InvoiceId == invoiceId.Value)
                .Load();
            var invoice = _context.Invoices.Find(invoiceId.Value);
            _context.Entry(invoice).Collection(i => i.LineItems).Load();
            var payments = _context.Transactions.Local
                .Where(t => t.InvoiceId == invoiceId.Value && (!transactionToIgnore.HasValue || t.TransactionId != transactionToIgnore.Value))
                .Sum(t => t.Amount);
            invoice.Balance = invoice.LineItems.Sum(li => li.Amount) - payments;
            _context.Invoices.Update(invoice);
        }

        public void Delete(Guid transactionId)
        {
            var transaction = _context.Transactions
                .Include(t => t.LeftTransfer)
                .Include(t => t.RightTransfer)
                .Include(t => t.LeftTransfer.RightTransaction)
                .Single(t => t.TransactionId == transactionId);
            UpdateInvoiceBalance(transaction.InvoiceId, transactionId);
            _context.Transactions.Remove(transaction);
            if (transaction.LeftTransfer != null)
            {
                _context.Transactions.Remove(transaction.LeftTransfer.RightTransaction);
                _context.Transfers.Remove(transaction.LeftTransfer);
                _context.Transfers.Remove(transaction.RightTransfer);
            }
            _context.SaveChanges();
        }

        public Transaction Update(TransactionForDisplay transaction)
        {
            var dbTransaction = _context.Transactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .Include(t => t.RightTransfer)
                .Single(t => t.TransactionId == transaction.TransactionId);
            if (TransactionTypeChanged(transaction, dbTransaction))
            {
                throw new Exception("Can't change the type of the transaction");
            }
            if (IsTransferAndAccountChanged(transaction, dbTransaction))
            {
                throw new Exception("Can't change the destination account of a transfer");
            }
            dbTransaction.AccountId = transaction.AccountId.Value;
            dbTransaction.Vendor = transaction.Vendor;
            dbTransaction.Description = transaction.Description;
            dbTransaction.IsReconciled = transaction.IsReconciled;
            dbTransaction.InvoiceId = transaction.InvoiceId;
            dbTransaction.TransactionDate = transaction.TransactionDate;
            if (transaction.CategoryId.HasValue)
                dbTransaction.CategoryId = transaction.CategoryId;

            UpdateAmount(dbTransaction, transaction);

            _context.Transactions.Update(dbTransaction);
            UpdateInvoiceBalance(transaction.InvoiceId);
            AddOrUpdateVendor(transaction.Vendor, transaction.CategoryId);
            _context.SaveChanges();
            return dbTransaction;
        }

        private bool IsTransferAndAccountChanged(TransactionForDisplay transaction, Transaction dbTransaction)
        {
            if (dbTransaction.TransactionType != TRANSACTION_TYPE.TRANSFER && dbTransaction.TransactionType != TRANSACTION_TYPE.INVESTMENT)
                return false;
            if (dbTransaction.LeftTransfer.RightTransaction.AccountId != transaction.RelatedAccountId)
                return false;
            return true;
        }

        private bool TransactionTypeChanged(TransactionForDisplay transaction, Transaction dbTransaction)
        {
            return dbTransaction.TransactionType != transaction.TransactionType;
        }

        private void UpdateAmount(Transaction dbTransaction, TransactionForDisplay transaction)
        {
            if (dbTransaction.Amount == transaction.Amount) return;

            LoadCadExchangeRate();
            TransactionAmountUpdater updater;
            switch (dbTransaction.TransactionType)
            {
                case TRANSACTION_TYPE.REGULAR:
                case TRANSACTION_TYPE.INVOICE_PAYMENT:
                case TRANSACTION_TYPE.MORTGAGE_PAYMENT:
                case TRANSACTION_TYPE.DIVIDEND:
                    updater = new TransactionAmountUpdaterRegular(dbTransaction, _cadExchangeRate);
                    updater.UpdateAmount(transaction.Amount);
                    break;
                case TRANSACTION_TYPE.TRANSFER:
                case TRANSACTION_TYPE.INVESTMENT:
                    _context.Entry(dbTransaction).Reference(t => t.LeftTransfer).Load();
                    _context.Entry(dbTransaction.LeftTransfer).Reference(t => t.RightTransaction).Load();
                    updater = new TransactionAmountUpdaterTransfer(dbTransaction, _cadExchangeRate);
                    var relatedTransaction = updater.UpdateAmount(transaction.Amount);
                    _context.Transactions.Update(relatedTransaction);
                    break;
            }
        }

        private void AddOrUpdateVendor(string vendorName, Guid? categoryId)
        {
            if (!categoryId.HasValue) return;
            if (string.IsNullOrWhiteSpace(vendorName)) return;
            var vendor = _context.Vendors.SingleOrDefault(v => v.Name.ToLower() == vendorName.ToLower());
            if (vendor == null)
            {
                vendor = new Vendor
                {
                    VendorId = Guid.NewGuid(),
                    Name = vendorName,
                    LastTransactionCategoryId = categoryId.Value
                };
                _context.Vendors.Add(vendor);
            }
            else
            {
                vendor.LastTransactionCategoryId = categoryId.Value;
            }
        }

        private void LoadCadExchangeRate()
        {
            if (_cadExchangeRate > decimal.MinValue) return;
            _cadExchangeRate = _context.Currencies.GetCadExchangeRate();
        }

        public IEnumerable<Transaction> Insert(TransactionForDisplay transactionDto)
        {
            LoadCadExchangeRate();
            var transactionList = new List<Transaction>();
            var transaction = transactionDto.ShallowMap();
            transaction.Category = _context.Categories.Find(transaction.CategoryId);
            var exchangeRate = 1.0m;
            if (GetCurrencyFor(transaction.AccountId) == "CAD")
            {
                exchangeRate = _cadExchangeRate;
            };
            transaction.AmountInBaseCurrency = Math.Round(transaction.Amount / exchangeRate, 2);
            transactionList.Add(transaction);
            _context.Transactions.Add(transaction);
            var bankFeeTransactions = GetBankFeeTransactions(transactionDto);
            foreach (var trx in bankFeeTransactions)
            {
                trx.AmountInBaseCurrency = Math.Round(trx.Amount / exchangeRate, 2);
                _context.Transactions.Add(trx);
            }
            transactionList.AddRange(bankFeeTransactions);
            UpdateInvoiceBalance(transaction.InvoiceId);
            AddOrUpdateVendor(transactionDto.Vendor, transactionDto.CategoryId);
            if (transactionDto.TransactionType == TRANSACTION_TYPE.TRANSFER)
            {
                CreateTransferFrom(transactionDto, transaction.TransactionId);
            }
            _context.SaveChanges();
            return transactionList;
        }

        private string GetCurrencyFor(Guid accountId)
        {
            return _context.Accounts.Find(accountId).Currency;
        }

        private void CreateTransferFrom(TransactionForDisplay transactionDto, Guid relatedTransactionId)
        {
            var rightTransaction = transactionDto.ShallowMap();
            rightTransaction.TransactionId = Guid.NewGuid();
            rightTransaction.AccountId = transactionDto.RelatedAccountId.Value;
            rightTransaction.Amount = 0 - transactionDto.Amount;
            LoadCadExchangeRate();
            var sourceCurrency = GetCurrencyFor(transactionDto.AccountId.Value);
            var destCurrency = GetCurrencyFor(transactionDto.RelatedAccountId.Value);
            if (sourceCurrency == "USD" && destCurrency == "CAD")
            {
                rightTransaction.AmountInBaseCurrency = rightTransaction.Amount;
                rightTransaction.Amount = Math.Round(rightTransaction.Amount * _cadExchangeRate, 2);
            }
            else if (sourceCurrency == "CAD" && destCurrency == "USD")
            {
                rightTransaction.AmountInBaseCurrency = Math.Round(rightTransaction.Amount / _cadExchangeRate, 2);
                rightTransaction.Amount = Math.Round(rightTransaction.Amount / _cadExchangeRate, 2);
            }
            else if (sourceCurrency == "CAD" && destCurrency == "CAD")
            {
                rightTransaction.AmountInBaseCurrency = Math.Round(rightTransaction.Amount / _cadExchangeRate, 2);
            }
            else
            {
                rightTransaction.AmountInBaseCurrency = rightTransaction.Amount;
            }
            _context.Transactions.Add(rightTransaction);
            _context.Transfers.Add(new Transfer
            {
                TransferId = Guid.NewGuid(),
                LeftTransactionId = relatedTransactionId,
                RightTransactionId = rightTransaction.TransactionId
            });
            _context.Transfers.Add(new Transfer
            {
                TransferId = Guid.NewGuid(),
                RightTransactionId = relatedTransactionId,
                LeftTransactionId = rightTransaction.TransactionId
            });
        }

        public TransactionListModel GetByAccount(Guid accountId, int? page)
        {
            var thePage = page ?? 0;
            var transactionList = _context.Transactions
                .Include(t => t.LeftTransfer)
                .Include(t => t.LeftTransfer.RightTransaction)
                .Include(t => t.LeftTransfer.RightTransaction.Account)
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.EnteredDate)
                .ThenBy(t => t.TransactionId)
                .Skip(PAGE_SIZE * thePage).Take(PAGE_SIZE)
                .ToList();
            var transactions = transactionList
                .Select(t => _mapper.Map<TransactionForDisplay>(t))
                .ToList();

            var remainingTransactionCount = _context.Transactions
                .Count(t => t.AccountId == accountId) - transactions.Count();
            var startingBalance = _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.EnteredDate)
                .ThenBy(t => t.TransactionId)
                .Skip(transactionList.Count())
                .Sum(t => t.Amount);
            var runningTotal = _context.Transactions
                .Where(t => t.AccountId == accountId)
                .Sum(t => t.Amount);
            transactions.ForEach(t => {
                t.SetDebitAndCredit();
                t.RunningTotal = runningTotal;
                runningTotal -= t.Amount;
            });

            var model = new TransactionListModel
            {
                Transactions = transactions,
                StartingBalance = startingBalance,
                RemainingTransactionCount = remainingTransactionCount,
                Page = thePage
            };
            return model;
        }

        public IEnumerable<TransactionForDisplay> GetByAccount(Guid accountId)
        {
            var transactions = _context.Transactions
                .Include(t => t.LeftTransfer)
                .Include(t => t.LeftTransfer.RightTransaction)
                .Include(t => t.LeftTransfer.RightTransaction.Account)
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.EnteredDate)
                .Select(t => _mapper.Map<TransactionForDisplay>(t))
                .ToList();
            var runningTotal = _context.Transactions
                .Where(t => t.AccountId == accountId)
                .Sum(t => t.Amount);
            transactions.ForEach(t => {
                t.SetDebitAndCredit();
                t.RunningTotal = runningTotal;
                runningTotal -= t.Amount;
            });
            return transactions;
        }

        public TransactionForDisplay Get(Guid transactionId)
        {
            var transaction = _context.Transactions
                .Include(t => t.LeftTransfer)
                .Include(t => t.LeftTransfer.RightTransaction)
                .Include(t => t.LeftTransfer.RightTransaction.Account)
                .Include(t => t.Category)
                .Include(t => t.Account)
                .SingleOrDefault(t => t.TransactionId == transactionId);
            var mapped = _mapper.Map<TransactionForDisplay>(transaction);
            mapped.SetDebitAndCredit();
            return mapped;
        }

        private IEnumerable<Transaction> GetBankFeeTransactions(TransactionForDisplay newTransaction)
        {
            var transactions = new List<Transaction>();
            var description = newTransaction.Description;
            if (!description.Contains("bf:", StringComparison.CurrentCultureIgnoreCase))
            {
                return transactions;
            }

            var category = _context.GetOrCreateCategory("Bank Fees").GetAwaiter().GetResult();
            var vendor = _context.Accounts.Find(newTransaction.AccountId.Value).Vendor;
            newTransaction.Description = description.Substring(0, description.IndexOf("bf:", StringComparison.CurrentCultureIgnoreCase));
            var parsed = description.Substring(description.IndexOf("bf:", 0, StringComparison.CurrentCultureIgnoreCase));
            while (parsed.StartsWith("bf:", StringComparison.CurrentCultureIgnoreCase))
            {
                var next = parsed.IndexOf("bf:", 1, StringComparison.CurrentCultureIgnoreCase);
                if (next == -1) next = parsed.Length;
                var transactionData = parsed[3..next].Trim().Split(" ");
                if (decimal.TryParse(transactionData[0], out var amount))
                {
                    var bankFeeDescription = string.Join(" ", transactionData.Skip(1).ToArray());
                    var transaction = new Transaction
                    {
                        TransactionId = Guid.NewGuid(),
                        TransactionDate = newTransaction.TransactionDate,
                        AccountId = newTransaction.AccountId.Value,
                        CategoryId = category.CategoryId,
                        Category = category,
                        Description = bankFeeDescription,
                        Vendor = vendor,
                        Amount = 0 - amount,
                        EnteredDate = newTransaction.EnteredDate
                    };
                    transactions.Add(transaction);
                }
                parsed = parsed.Substring(next);
            }
            return transactions;
        }

    }
}
