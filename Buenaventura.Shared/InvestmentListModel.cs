namespace Buenaventura.Shared;

public class InvestmentListModel
{
    public IEnumerable<InvestmentModel> Investments { get; set; } = [];
    public double PortfolioIrr { get; set; }
}