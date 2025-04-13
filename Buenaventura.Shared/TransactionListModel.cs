namespace Buenaventura.Shared 
{
    public class TransactionListModel : PaginatedResults<TransactionForDisplay>
    {
        public decimal StartingBalance { get; set; }
    }
}