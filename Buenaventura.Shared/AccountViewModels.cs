using Microsoft.AspNetCore.Http;

namespace Buenaventura.Shared
{

  public class AccountForPosting
    {
        public Guid AccountId { get; set; }
        public string Name { get; set; }
        public decimal StartingBalance { get; set; }
        public DateTime StartDate { get; set; }
        public string Currency { get; set; }
        public string Vendor { get; set; }
        public string AccountType { get; set; }
        public decimal? MortgagePayment { get; set; }
        public string MortgageType { get; set; }
        public bool IsHidden { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class AccountWithTransactions
    {
        public string Name { get; set; }
        public Guid AccountId { get; set; }
        public decimal CurrentBalance { get; set; }
        public IEnumerable<TransactionForDisplay> Transactions { get; set; }
    }

    public class AccountQifViewModel
    {
        public IFormFile File { get; set; }
        public Guid AccountId { get; set; }
        public DateTime? FromDate { get; set; }
        public string Transactions{ get; set; }
    }

    public class QifTransaction
    {
        public DateTime? Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public string Vendor { get; set; }
        public bool Reconciled { get; set; }
    }

    public class AccountIdAndBalance {
        public Guid AccountId { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal CurrentBalanceInUsd { get; set; }
    }

    public class AccountWithBalance
    {
        public Guid AccountId { get; set; }
        public string Name { get; set; }
        public decimal StartingBalance { get; set; }
        public DateTime StartDate { get; set; }
        public string Currency { get; set; }
        public string Vendor { get; set; }
        public string AccountType { get; set; }
        public decimal? MortgagePayment { get; set; }
        public string MortgageType { get; set; }
        public bool IsHidden { get; set; }
        public int DisplayOrder { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal CurrentBalanceInUsd { get; set; }
    }
}
