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
        var transactionsByDate = Transactions.OrderBy(t => t.Date).ToList();
        var startDate = transactionsByDate.First().Date;
        var payments = new List<double>();
        var days = new List<double>();
        foreach (var transaction in transactionsByDate)
        {
            payments.Add(Math.Round(-Convert.ToDouble(transaction.Shares) * Convert.ToDouble(transaction.Price), 4));
            days.Add((transaction.Date - startDate).Days);
        }

        if (Dividends != null)
        {
            var dividendsByDate = Dividends.OrderBy(t => t.TransactionDate);
            foreach (var dividend in dividendsByDate)
            {
                payments.Add(Convert.ToDouble(dividend.Amount));
                days.Add((dividend.TransactionDate - startDate).Days);
            }
        }

        payments.Add(Convert.ToDouble(GetCurrentValue()));
        days.Add((DateTime.Today - startDate).Days);
        var irr = Irr.CalculateIrr(payments.ToArray(), days.ToArray());
        if (double.IsPositiveInfinity(irr))
        {
            irr = 1_000_000;
        }

        if (double.IsNegativeInfinity(irr))
        {
            irr = -1_000_000;
        }

        if (double.IsNaN(irr))
        {
            irr = 0;
        }
        return irr;
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