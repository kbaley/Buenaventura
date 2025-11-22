using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("investments")]
public class Investment
{
    [Key] public Guid InvestmentId { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public string Currency { get; set; }
    public bool DontRetrievePrices { get; set; }
    public virtual ICollection<InvestmentTransaction> Transactions { get; set; }
    public decimal LastPrice { get; set; }
    public DateTime LastPriceRetrievalDate { get; set; }
    public Guid? CategoryId { get; set; }
    public InvestmentCategory? Category { get; set; }
    public bool PaysDividends { get; set; }
    [ForeignKey("DividendInvestmentId")] public virtual IEnumerable<Transaction> Dividends { get; set; }

    public decimal GetTotalReturn()
    {
        var totalPaid = Transactions.Sum(t => t.Shares * t.Price);
        var dividends = Dividends.Sum(d => d.Amount);
        var currentValue = GetCurrentValue();
        return currentValue == 0 ? 0m : (currentValue + dividends - totalPaid) / currentValue;
    }

    public double GetAnnualizedIrr()
    {
        if (Transactions.Count == 0) return 0.0;
        
        // Find the start of the current holding period
        var transactionsByDate = Transactions.OrderBy(t => t.Date).ToList();
        var currentHoldingStartDate = FindCurrentHoldingStartDate(transactionsByDate);
        
        // Only consider transactions from the current holding period
        var relevantTransactions = transactionsByDate.Where(t => t.Date >= currentHoldingStartDate).ToList();
        if (relevantTransactions.Count == 0) return 0.0;
        
        var startDate = relevantTransactions.First().Date;
        var payments = new List<double>();
        var days = new List<double>();
        
        foreach (var transaction in relevantTransactions)
        {
            payments.Add(-Convert.ToDouble(transaction.Shares) * Convert.ToDouble(transaction.Price));
            days.Add((transaction.Date - startDate).Days);
        }

        if (Dividends != null)
        {
            // Only include dividends from the current holding period
            var relevantDividends = Dividends.Where(d => d.TransactionDate >= currentHoldingStartDate).OrderBy(t => t.TransactionDate);
            foreach (var dividend in relevantDividends)
            {
                payments.Add(Convert.ToDouble(dividend.Amount));
                days.Add((dividend.TransactionDate - startDate).Days);
            }
        }

        payments.Add(Convert.ToDouble(GetCurrentValue()));
        days.Add((DateTime.Today - startDate).Days);
        var irr = Irr.CalculateIrr(payments.ToArray(), days.ToArray());
        return irr;
    }

    private DateTime FindCurrentHoldingStartDate(List<InvestmentTransaction> transactionsByDate)
    {
        // If we currently have no shares, return the date of the last transaction
        var currentShares = GetNumberOfShares();
        if (currentShares == 0)
        {
            return transactionsByDate.LastOrDefault()?.Date ?? DateTime.Today;
        }

        // Work backwards from the most recent transaction to find when we first acquired current holdings
        var runningShares = 0m;
        var holdingStartDate = DateTime.Today;

        // Process transactions in reverse chronological order
        for (int i = transactionsByDate.Count - 1; i >= 0; i--)
        {
            var transaction = transactionsByDate[i];
            runningShares += transaction.Shares;
            holdingStartDate = transaction.Date;

            // If we've accounted for all current shares, this is our start date
            if (runningShares >= currentShares)
            {
                break;
            }

            // If we went to zero or negative shares at any point, 
            // it means we had sold everything and re-entered
            if (runningShares <= 0)
            {
                // The next transaction forward is where current holdings began
                holdingStartDate = i < transactionsByDate.Count - 1 
                    ? transactionsByDate[i + 1].Date 
                    : transaction.Date;
                break;
            }
        }

        return holdingStartDate;
    }

    public decimal GetNumberOfShares()
    {
        if (Transactions == null) return 0;
        return Transactions.Sum(t => t.Shares);
    }

    public decimal GetAveragePricePaid()
    {
        var purchaseTransactions = Transactions.Where(t => t.Shares > 0);
        var numShares = purchaseTransactions.Sum(t => t.Shares);
        if (numShares == 0) return 0;
        return purchaseTransactions.Sum(t => t.Shares * t.Price) / numShares;
    }

    public decimal GetCurrentValue()
    {
        return GetNumberOfShares() * LastPrice;
    }
}