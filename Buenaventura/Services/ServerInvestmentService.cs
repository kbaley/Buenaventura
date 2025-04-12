using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerInvestmentService(IDbContextFactory<CoronadoDbContext> contextFactory) : IInvestmentService
{
    public async Task<InvestmentListModel> GetInvestments()
    {
        var context = await contextFactory.CreateDbContextAsync();
        var investments = await context.Investments
            .Include(i => i.Transactions)
            .Include(i => i.Dividends)
            .Select(i => new InvestmentForListDto
            {
                InvestmentId = i.InvestmentId,
                Name = i.Name,
                Symbol = i.Symbol,
                Shares = i.Transactions.Sum(t => t.Shares),
                TotalSharesBought = i.Transactions.Where(t => t.Shares > 0).Sum(t => t.Shares),
                LastPrice = i.LastPrice,
                // Don't divide by number of shares yet in case it's zero; we'll do this later
                AveragePrice = i.Transactions.Where(t => t.Shares > 0).Sum(t => t.Shares * t.Price),
                Currency = i.Currency,
                DontRetrievePrices = i.DontRetrievePrices,
                AnnualizedIrr = i.GetAnnualizedIrr(),
                CategoryId = i.CategoryId,
                PaysDividends = i.PaysDividends
            })
            .Where(i => i.Shares != 0)
            .OrderBy(i => i.Name)
            .ToListAsync();
        investments.ForEach(i =>
        {
            i.CurrentValue = i.Shares * i.LastPrice;
            i.AveragePrice = i.Shares == 0 ? 0 : i.AveragePrice / i.TotalSharesBought;
            i.BookValue = i.Shares * i.AveragePrice;
        });
        var totalIrr = await context.Investments.GetAnnualizedIrr();

        return new InvestmentListModel
        {
            Investments = investments,
            PortfolioIrr = totalIrr
        };
    }
}