namespace Buenaventura.Dtos;

public class InvestmentListModel
{
    public IEnumerable<InvestmentForListDto> Investments { get; set; }
    public double PortfolioIrr { get; set; }
}