using System.Text.Json.Serialization;

namespace Buenaventura.Shared
{
    public class CategoryTotal
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = "";

        public IList<MonthlyAmount> Amounts { get; set; } = [];
        public decimal Total { get; set; }

        public void Merge(CategoryTotal other) {
            foreach (var item in other.Amounts)
            {
                var match = Amounts.SingleOrDefault(a => a.Date == item.Date);
                if (match == null) {
                    Amounts.Add(item);
                } else {
                    match.Amount += item.Amount;
                }
            }
        }
    }

    [method: JsonConstructor]
    public class MonthlyAmount(int year, int month, decimal amount)
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; } = new(year, month, 1);
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; } = amount;

        /// <summary>
        /// Required for JSON deserialization
        /// </summary>
        public int Year { get; set; } = year;
        /// <summary>
        /// Required for JSON deserialization
        /// </summary>
        public int Month { get; set; } = month;
    }
    
    public class CategoryTotals
    {
        public IEnumerable<CategoryTotal> Expenses { get; set; } = [];
        public IEnumerable<MonthlyAmount>? MonthTotals { get; set; }
    }
}