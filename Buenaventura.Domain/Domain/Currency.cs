using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("currencies")]
public class Currency
{
    [Key] public Guid CurrencyId { get; set; }

    public string Symbol { get; set; }
    public DateTime LastRetrieved { get; set; }
    public decimal PriceInUsd { get; set; }
}