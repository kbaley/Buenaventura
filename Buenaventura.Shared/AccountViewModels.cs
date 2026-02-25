namespace Buenaventura.Shared
{
    public class AccountForPosting
    {
        public Guid AccountId { get; set; }
        public string Name { get; set; } = "";
        public decimal StartingBalance { get; set; }
        public DateTime StartDate { get; set; }
        public string Currency { get; set; } = "";
        public string Vendor { get; set; } = "";
        public string AccountType { get; set; } = "";
        public decimal? MortgagePayment { get; set; }
        public string MortgageType { get; set; } = "";
        public bool IsHidden { get; set; }
        public bool ExcludeFromReports { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class AccountWithTransactions
    {
        public string Name { get; set; } = "";
        public Guid AccountId { get; set; }
        public decimal CurrentBalance { get; set; }
        public IEnumerable<TransactionForDisplay> Transactions { get; set; } = [];
    }

    public class AccountIdAndBalance
    {
        public Guid AccountId { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal CurrentBalanceInUsd { get; set; }
    }

    public class AccountWithBalance
    {
        public Guid AccountId { get; set; }
        public string Name { get; set; } = "";
        public decimal StartingBalance { get; set; }
        public DateTime StartDate { get; set; }
        public string Currency { get; set; } = "USD";
        public string Vendor { get; set; } = "";
        public string AccountType { get; set; } = "";
        public decimal? MortgagePayment { get; set; }
        public string MortgageType { get; set; } = "";
        public bool IsHidden { get; set; }
        public bool ExcludeFromReports { get; set; }
        public int DisplayOrder { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal CurrentBalanceInUsd { get; set; }
    }
}
