using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IAccountService : IAppService
{
     Task<IEnumerable<AccountWithBalance>> GetAccounts();
     Task<TransactionListModel> GetTransactions(Guid accountId, string search = "", int page = 0, int pageSize = 50);
     Task<AccountWithBalance> GetAccount(Guid id);
     Task UpdateTransaction(TransactionForDisplay transaction);
     Task AddTransaction(Guid accountId, TransactionForDisplay transaction);
     Task DeleteTransaction(Guid transactionId);
     Task SaveAccountOrder(List<OrderedAccount> accountOrders);
     Task<TransactionListModel> GetPotentialDuplicateTransactions(Guid accountId);
}