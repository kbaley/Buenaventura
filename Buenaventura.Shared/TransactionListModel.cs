using Buenaventura.Shared;

namespace Buenaventura.Dtos 
{
    public class TransactionListModel {
        public IEnumerable<TransactionForDisplay> Transactions { get; set; }
        public int Page { get; set; }
        public int RemainingTransactionCount { get; set; }
        public int TotalTransactionCount { get; set; }

        public decimal StartingBalance { get; set; }
    }
}