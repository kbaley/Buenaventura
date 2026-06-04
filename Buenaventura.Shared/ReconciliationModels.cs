namespace Buenaventura.Shared;

public enum ReconciliationMode
{
    Statement,
    OnlineBalance
}

public class ReconciliationWorkspace
{
    public AccountWithBalance Account { get; set; } = new();
    public DateTime AsOfDate { get; set; } = DateTime.Today;
    public decimal ReconciledBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public List<TransactionForDisplay> Transactions { get; set; } = [];
}

public class CompleteReconciliationRequest
{
    public Guid AccountId { get; set; }
    public ReconciliationMode Mode { get; set; }
    public DateTime AsOfDate { get; set; }
    public decimal TargetBalance { get; set; }
    public List<Guid> TransactionIds { get; set; } = [];
}
