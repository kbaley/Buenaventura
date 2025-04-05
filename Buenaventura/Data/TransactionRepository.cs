using AutoMapper;
using Buenaventura.Domain;
using Buenaventura.Dtos;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Data
{
    public class TransactionRepository(CoronadoDbContext context, IMapper mapper) : ITransactionRepository
    {
        private decimal _cadExchangeRate = decimal.MinValue;

        private async Task UpdateInvoiceBalance(Guid? invoiceId, Guid? transactionToIgnore = null)
        {
            if (!invoiceId.HasValue) return;

            // Load all invoice transactions into the cache so that we can work locally.
            // This is because we might have added or updated a related transaction that
            // hasn't been committed to the database yet.
            await context.Transactions
                .Where(t => t.InvoiceId == invoiceId.Value)
                .LoadAsync();
            var invoice = await context.Invoices.FindAsync(invoiceId.Value);
            if (invoice == null) return;
            await context.Entry(invoice).Collection(i => i.LineItems).LoadAsync();
            var payments = context.Transactions.Local
                .Where(t => t.InvoiceId == invoiceId.Value && (!transactionToIgnore.HasValue || t.TransactionId != transactionToIgnore.Value))
                .Sum(t => t.Amount);
            invoice.Balance = invoice.LineItems.Sum(li => li.Amount) - payments;
            context.Invoices.Update(invoice);
        }

        public async Task Delete(Guid transactionId)
        {
            var transaction = await context.Transactions
                .Include(t => t.LeftTransfer)
                .Include(t => t.RightTransfer)
                .Include(t => t.LeftTransfer.RightTransaction)
                .SingleAsync(t => t.TransactionId == transactionId);
            await UpdateInvoiceBalance(transaction.InvoiceId, transactionId);
            context.Transactions.Remove(transaction);
            if (transaction.LeftTransfer != null)
            {
                context.Transactions.Remove(transaction.LeftTransfer.RightTransaction!);
                context.Transfers.Remove(transaction.LeftTransfer);
                context.Transfers.Remove(transaction.RightTransfer);
            }
            await context.SaveChangesAsync();
        }

        public async Task<Transaction> Update(TransactionForDisplay transaction)
        {
            var dbTransaction = await context.Transactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .Include(t => t.RightTransfer)
                .SingleAsync(t => t.TransactionId == transaction.TransactionId);
            if (TransactionTypeChanged(transaction, dbTransaction))
            {
                throw new Exception("Can't change the type of the transaction");
            }
            if (IsTransferAndAccountChanged(transaction, dbTransaction))
            {
                throw new Exception("Can't change the destination account of a transfer");
            }
            dbTransaction.AccountId = transaction.AccountId!.Value;
            dbTransaction.Vendor = transaction.Vendor;
            dbTransaction.Description = transaction.Description;
            dbTransaction.IsReconciled = transaction.IsReconciled;
            dbTransaction.InvoiceId = transaction.InvoiceId;
            dbTransaction.TransactionDate = transaction.TransactionDate;
            if (transaction.CategoryId.HasValue)
                dbTransaction.CategoryId = transaction.CategoryId;

            await UpdateAmount(dbTransaction, transaction);

            context.Transactions.Update(dbTransaction);
            await UpdateInvoiceBalance(transaction.InvoiceId);
            AddOrUpdateVendor(transaction.Vendor!, transaction.CategoryId);
            await context.SaveChangesAsync();
            return dbTransaction;
        }

        private bool IsTransferAndAccountChanged(TransactionForDisplay transaction, Transaction dbTransaction)
        {
            if (dbTransaction.TransactionType != TransactionType.TRANSFER && dbTransaction.TransactionType != TransactionType.INVESTMENT)
                return false;
            if (dbTransaction.LeftTransfer.RightTransaction!.AccountId != transaction.RelatedAccountId)
                return false;
            return true;
        }

        private bool TransactionTypeChanged(TransactionForDisplay transaction, Transaction dbTransaction)
        {
            return dbTransaction.TransactionType != transaction.TransactionType;
        }

        private async Task UpdateAmount(Transaction dbTransaction, TransactionForDisplay transaction)
        {
            if (dbTransaction.Amount == transaction.Amount) return;

            await LoadCadExchangeRate();
            TransactionAmountUpdater updater;
            switch (dbTransaction.TransactionType)
            {
                case TransactionType.REGULAR:
                case TransactionType.INVOICE_PAYMENT:
                case TransactionType.MORTGAGE_PAYMENT:
                case TransactionType.DIVIDEND:
                    updater = new TransactionAmountUpdaterRegular(dbTransaction, _cadExchangeRate);
                    updater.UpdateAmount(transaction.Amount);
                    break;
                case TransactionType.TRANSFER:
                case TransactionType.INVESTMENT:
                    await context.Entry(dbTransaction).Reference(t => t.LeftTransfer).LoadAsync();
                    await context.Entry(dbTransaction.LeftTransfer).Reference(t => t.RightTransaction).LoadAsync();
                    updater = new TransactionAmountUpdaterTransfer(dbTransaction, _cadExchangeRate);
                    var relatedTransaction = updater.UpdateAmount(transaction.Amount);
                    context.Transactions.Update(relatedTransaction);
                    break;
            }
        }

        private void AddOrUpdateVendor(string vendorName, Guid? categoryId)
        {
            if (!categoryId.HasValue) return;
            if (string.IsNullOrWhiteSpace(vendorName)) return;
            var vendor = context.Vendors.SingleOrDefault(v => v.Name.ToLower() == vendorName.ToLower());
            if (vendor == null)
            {
                vendor = new Vendor
                {
                    VendorId = Guid.NewGuid(),
                    Name = vendorName,
                    LastTransactionCategoryId = categoryId.Value
                };
                context.Vendors.Add(vendor);
            }
            else
            {
                vendor.LastTransactionCategoryId = categoryId.Value;
            }
        }

        private async Task LoadCadExchangeRate()
        {
            if (_cadExchangeRate > decimal.MinValue) return;
            _cadExchangeRate = await context.Currencies.GetCadExchangeRate();
        }

        public async Task<IEnumerable<Transaction>> Insert(TransactionForDisplay transactionDto)
        {
            await LoadCadExchangeRate();
            var transactionList = new List<Transaction>();
            var transaction = transactionDto.ShallowMap();
            transaction.Category = (await context.Categories.FindAsync(transaction.CategoryId))!;
            var exchangeRate = 1.0m;
            if (GetCurrencyFor(transaction.AccountId) == "CAD")
            {
                exchangeRate = _cadExchangeRate;
            }

            transaction.AmountInBaseCurrency = Math.Round(transaction.Amount / exchangeRate, 2);
            transactionList.Add(transaction);
            context.Transactions.Add(transaction);
            var bankFeeTransactions = (await GetBankFeeTransactions(transactionDto)).ToList();
            foreach (var trx in bankFeeTransactions)
            {
                trx.AmountInBaseCurrency = Math.Round(trx.Amount / exchangeRate, 2);
                context.Transactions.Add(trx);
            }
            transactionList.AddRange(bankFeeTransactions);
            await UpdateInvoiceBalance(transaction.InvoiceId);
            AddOrUpdateVendor(transactionDto.Vendor!, transactionDto.CategoryId);
            if (transactionDto.TransactionType == TransactionType.TRANSFER)
            {
                await CreateTransferFrom(transactionDto, transaction.TransactionId);
            }
            await context.SaveChangesAsync();
            return transactionList;
        }

        private string GetCurrencyFor(Guid accountId)
        {
            return context.Accounts.Find(accountId)!.Currency;
        }

        private async Task CreateTransferFrom(TransactionForDisplay transactionDto, Guid relatedTransactionId)
        {
            var rightTransaction = transactionDto.ShallowMap();
            rightTransaction.TransactionId = Guid.NewGuid();
            rightTransaction.AccountId = transactionDto.RelatedAccountId!.Value;
            rightTransaction.Amount = 0 - transactionDto.Amount;
            await LoadCadExchangeRate();
            var sourceCurrency = GetCurrencyFor(transactionDto.AccountId!.Value);
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
            context.Transactions.Add(rightTransaction);
            context.Transfers.Add(new Transfer
            {
                TransferId = Guid.NewGuid(),
                LeftTransactionId = relatedTransactionId,
                RightTransactionId = rightTransaction.TransactionId
            });
            context.Transfers.Add(new Transfer
            {
                TransferId = Guid.NewGuid(),
                RightTransactionId = relatedTransactionId,
                LeftTransactionId = rightTransaction.TransactionId
            });
        }

        public async Task<TransactionListModel> GetByAccount(Guid accountId, string search = "", int page = 0, int pageSize = 50)
        {
            var transactionList = await context.Transactions
                .Include(t => t.LeftTransfer)
                .Include(t => t.LeftTransfer.RightTransaction)
                .Include(t => t.LeftTransfer.RightTransaction!.Account)
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.AccountId == accountId
                    && (string.IsNullOrWhiteSpace(search) 
                        || t.Description.ToLower().Contains(search.ToLower())
                        || t.Vendor != null && t.Vendor.ToLower().Contains(search.ToLower())
                        || t.Category.Name.ToLower().Contains(search.ToLower()))
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                        || t.Amount.ToString() == search
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                        || (-t.Amount).ToString() == search
                    )
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.EnteredDate)
                .ThenBy(t => t.TransactionId)
                .Skip(pageSize * page).Take(pageSize)
                .ToListAsync();
            var transactions = transactionList
                .Select(mapper.Map<TransactionForDisplay>)
                .ToList();

            var totalTransactionCount = await context.Transactions
                .CountAsync(t => t.AccountId == accountId
                            && (string.IsNullOrWhiteSpace(search)
                        || t.Description.ToLower().Contains(search.ToLower())
                        || t.Vendor != null && t.Vendor.ToLower().Contains(search.ToLower())
                        || t.Category.Name.ToLower().Contains(search.ToLower()))
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                        || t.Amount.ToString() == search
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                        || (-t.Amount).ToString() == search);
            var endingBalance = await context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.EnteredDate)
                .ThenBy(t => t.TransactionId)
                .Skip(pageSize * page)
                .SumAsync(t => t.Amount);
            var startingBalance = endingBalance - transactions.Sum(t => t.Amount);
            transactions.ForEach(t => {
                t.SetDebitAndCredit();
                t.RunningTotal = endingBalance;
                endingBalance -= t.Amount;
            });

            var model = new TransactionListModel
            {
                Transactions = transactions,
                StartingBalance = startingBalance,
                TotalTransactionCount = totalTransactionCount
            };
            return model;
        }

        public async Task<TransactionForDisplay> Get(Guid transactionId)
        {
            var transaction = await context.Transactions
                .Include(t => t.LeftTransfer)
                .Include(t => t.LeftTransfer.RightTransaction)
                .Include(t => t.LeftTransfer.RightTransaction!.Account)
                .Include(t => t.Category)
                .Include(t => t.Account)
                .SingleOrDefaultAsync(t => t.TransactionId == transactionId);
            var mapped = mapper.Map<TransactionForDisplay>(transaction);
            mapped.SetDebitAndCredit();
            return mapped;
        }

        private async Task<IEnumerable<Transaction>> GetBankFeeTransactions(TransactionForDisplay newTransaction)
        {
            var transactions = new List<Transaction>();
            var description = newTransaction.Description;
            if (!description.Contains("bf:", StringComparison.CurrentCultureIgnoreCase))
            {
                return transactions;
            }

            var category = context.GetOrCreateCategory("Bank Fees").GetAwaiter().GetResult();
            var vendor = ((await context.Accounts.FindAsync(newTransaction.AccountId!.Value))!).Vendor;
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
