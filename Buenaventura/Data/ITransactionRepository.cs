using Buenaventura.Client.Services;
using Buenaventura.Domain;
using Buenaventura.Dtos;
using Buenaventura.Services;
using Buenaventura.Shared;

namespace Buenaventura.Data
{
    public interface ITransactionRepository : IServerAppService
    {
        Task<TransactionListModel> GetByAccount(Guid accountId, string search = "", int page = 0, int pageSize = 50);
        Task Insert(TransactionForDisplay transaction);
        Task<Transaction> Update(TransactionForDisplay transaction);
        Task Delete(Guid transactionId);
        Task<TransactionForDisplay> Get(Guid transactionId);
        Task<TransactionListModel> GetInDateRange(Guid accountId, DateTime start, DateTime end);
    }
}