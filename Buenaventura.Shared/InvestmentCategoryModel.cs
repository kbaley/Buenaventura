namespace Buenaventura.Shared;

public class InvestmentCategoryModel
{
    public Guid InvestmentCategoryId { get; set; }
    public string Name { get; set; } = "";
    public decimal Percentage { get; set; }
}