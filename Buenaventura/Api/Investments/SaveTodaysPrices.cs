using Buenaventura.Data;
using Buenaventura.Shared;
using Buenaventura.Dtos;
using Buenaventura.Services;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

internal class SaveTodaysPrices(BuenaventuraDbContext context, IInvestmentService investmentService)
    : Endpoint<List<TodaysPriceDto>, InvestmentListModel>
{
    public override void Configure()
    {
        Post("/api/investments/savetodaysprices");
    }

    public override async Task HandleAsync(List<TodaysPriceDto> req, CancellationToken ct)
    {
        var investmentsFromDb = await context.Investments.ToListAsync(ct);
        foreach (var item in req)
        {
            var investment = investmentsFromDb.SingleOrDefault(i => i.InvestmentId == item.InvestmentId);
            if (investment != null)
            {
                investment.LastPriceRetrievalDate = DateTime.Today;
                investment.LastPrice = item.LastPrice;
            }
        }

        await context.SaveChangesAsync(ct);
        var investments = await investmentService.GetInvestments();
        await SendOkAsync(investments, ct);
    }
}