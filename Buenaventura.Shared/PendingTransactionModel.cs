using System;

namespace Buenaventura.Shared;

public class PendingTransactionModel
{
    public string? ReferenceNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? CardNumber { get; set; }
    public string? Description { get; set; }
    public string? Vendor { get; set; }
    public decimal Amount { get; set; }
    public bool IsSelected { get; set; } = true;
    public bool IsAlreadyProcessed { get; set; }
    public bool HasPotentialMatch { get; set; }
    public TransactionForDisplay? PotentialMatch { get; set; }
    
    // Helper properties for display
    public decimal? Credit => Amount < 0 ? Math.Abs(Amount) : null;
    public decimal? Debit => Amount >= 0 ? Amount : null;
    public string DebitFormatted => Debit.HasValue ? Debit.Value.ToString("N2") : string.Empty;
    public string CreditFormatted => Credit.HasValue ? Credit.Value.ToString("N2") : string.Empty;
    public string DateFormatted => TransactionDate.ToString("MM/dd/yyyy");
}
