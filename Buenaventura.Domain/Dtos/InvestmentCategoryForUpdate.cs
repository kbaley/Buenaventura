namespace Buenaventura.Dtos;

public class InvestmentCategoryForUpdate
{
    public string Status { get; set; } = "Unchanged";
    public Guid InvestmentCategoryId { get; set; }
    public string Name { get; set; } = "";
    public decimal Percentage { get; set; }
}