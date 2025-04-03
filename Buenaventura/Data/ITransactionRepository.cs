using Buenaventura.Client.Services;
using Buenaventura.Domain;
using Buenaventura.Dtos;
using Buenaventura.Services;
using Buenaventura.Shared;

namespace Buenaventura.Data
{
    public interface ITransactionRepository : IServerAppService
    {
        TransactionListModel GetByAccount(Guid accountId, int? page);
        IEnumerable<TransactionForDisplay> GetByAccount(Guid accountId);
        IEnumerable<Transaction> Insert(TransactionForDisplay transaction);
        Transaction Update(TransactionForDisplay transaction);
        void Delete(Guid transactionId);
        TransactionForDisplay Get(Guid transactionId);
    }
}