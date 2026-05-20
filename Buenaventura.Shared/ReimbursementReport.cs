namespace Buenaventura.Shared;

public class ReimbursementReport
{
    public ReimbursementSummary Summary { get; set; } = new();
    public IEnumerable<ReimbursementMonth> MonthlyRows { get; set; } = [];
    public IEnumerable<ReimbursementTransaction> RecentTransactions { get; set; } = [];
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
    public DateTime TransactionDate { get; set; }
    public string AccountName { get; set; } = "";
    public string Vendor { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal ReimbursementAmount { get; set; }
    public decimal RunningBalance { get; set; }
}
