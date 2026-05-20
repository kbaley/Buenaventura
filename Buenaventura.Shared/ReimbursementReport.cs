namespace Buenaventura.Shared;

public class ReimbursementReport
{
    public ReimbursementSummary Summary { get; set; } = new();
    public IEnumerable<ReimbursementMonth> MonthlyRows { get; set; } = [];
    public IEnumerable<ReimbursementTransaction> RecentTransactions { get; set; } = [];
    public IEnumerable<ReimbursementTransaction> UnsettledExpenses { get; set; } = [];
    public IEnumerable<ReimbursementTransaction> UnsettledRepayments { get; set; } = [];
    public IEnumerable<ReimbursementSettlementModel> Settlements { get; set; } = [];
}

public class ReimbursementSummary
{
    public string CategoryName { get; set; } = "To be reimbursed";
    public bool HasTransactions { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal ExpensesLast12Months { get; set; }
    public decimal RepaymentsLast12Months { get; set; }
    public decimal NetLast12Months { get; set; }
    public DateTime? OldestOutstandingDate { get; set; }
    public int? OldestOutstandingDays { get; set; }
    public int UnsettledExpenseCount { get; set; }
    public decimal UnsettledExpenseTotal { get; set; }
    public int UnsettledRepaymentCount { get; set; }
    public decimal UnsettledRepaymentTotal { get; set; }
    public DateTime? LastExpenseDate { get; set; }
    public DateTime? LastRepaymentDate { get; set; }
}

public class ReimbursementMonth
{
    public DateTime Month { get; set; }
    public decimal Expenses { get; set; }
    public decimal Repayments { get; set; }
    public decimal Net { get; set; }
    public decimal RunningBalance { get; set; }
}

public class ReimbursementTransaction
{
    public Guid TransactionId { get; set; }
    public Guid? ReimbursementSettlementId { get; set; }
    public string SettlementName { get; set; } = "";
    public bool IsSettlementClosed { get; set; }
    public DateTime TransactionDate { get; set; }
    public string AccountName { get; set; } = "";
    public string Vendor { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal ReimbursementAmount { get; set; }
    public decimal RunningBalance { get; set; }
}

public class ReimbursementSettlementModel
{
    public Guid ReimbursementSettlementId { get; set; }
    public string Name { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public decimal Expenses { get; set; }
    public decimal Repayments { get; set; }
    public decimal Difference { get; set; }
    public int TransactionCount { get; set; }
    public int MatchCount { get; set; }
}

public class CreateReimbursementSettlementRequest
{
    public string Name { get; set; } = "";
    public string Notes { get; set; } = "";
    public bool CloseImmediately { get; set; } = true;
    public IEnumerable<Guid> TransactionIds { get; set; } = [];
    public IEnumerable<CreateReimbursementMatchRequest> Matches { get; set; } = [];
}

public class CreateReimbursementMatchRequest
{
    public string Notes { get; set; } = "";
    public string AcceptedDifferenceReason { get; set; } = "";
    public IEnumerable<Guid> TransactionIds { get; set; } = [];
}
