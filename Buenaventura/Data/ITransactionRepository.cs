using Buenaventura.Client.Services;
using Buenaventura.Domain;
using Buenaventura.Dtos;
using Buenaventura.Services;
using Buenaventura.Shared;

namespace Buenaventura.Data
{
    public interface ITransactionRepository : IServerAppService
    {
        TransactionListModel GetByAccount(Guid accountId, string search = "", int page = 0, int pageSize = 50);
        Task<IEnumerable<Transaction>> Insert(TransactionForDisplay transaction);
        Transaction Update(TransactionForDisplay transaction);
        void Delete(Guid transactionId);
        TransactionForDisplay Get(Guid transactionId);
    }
}