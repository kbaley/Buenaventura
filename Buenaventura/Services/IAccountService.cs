using Buenaventura.Client.Services;
using Buenaventura.Shared;

namespace Buenaventura.Services;

public interface IAccountService : IServerAppService
{
     Task<IEnumerable<AccountWithBalance>> GetAccounts();
     Task<TransactionListModel> GetTransactions(Guid accountId, string search = "", int page = 0, int pageSize = 50);
     Task<AccountWithBalance> GetAccount(Guid id);
     Task UpdateTransaction(TransactionForDisplay transaction);
     Task AddTransaction(Guid accountId, TransactionForDisplay transaction);
     Task DeleteTransaction(Guid transactionId);
     Task SaveAccountOrder(List<OrderedAccount> accountOrders);
     Task<TransactionListModel> GetPotentialDuplicateTransactions(Guid accountId);
     Task UpdateAccount(AccountWithBalance account);
     Task<TransactionListModel> GetAllTransactions(Guid accountId, DateTime startDate, DateTime endDate);
     Task<bool> AddBulkTransactions(Guid accountId, List<TransactionForDisplay> transactions);
}