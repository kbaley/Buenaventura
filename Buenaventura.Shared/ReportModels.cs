namespace Buenaventura.Shared
{
    public class CategoryTotal
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }

        public IList<DateAndAmount> Amounts { get; set; }
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

    public class DateAndAmount(int year, int month, decimal amount)
    {
        public DateTime Date { get; set; } = new(year, month, 1);
        public decimal Amount { get; set; } = amount;
    }
}