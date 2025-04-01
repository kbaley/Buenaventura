namespace Buenaventura.Dtos
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

    public class DateAndAmount
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }

        public DateAndAmount(int year, int month, decimal amount)
        {
            Date = new DateTime(year, month, 1);
            Amount = amount;
        }
    }
}