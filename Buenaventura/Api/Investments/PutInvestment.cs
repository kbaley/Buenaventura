using Buenaventura.Data;
using Buenaventura.Dtos;
using Buenaventura.Shared;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

internal class PutInvestment(BuenaventuraDbContext context)
    : Endpoint<InvestmentForUpdateDto, InvestmentModel>
{
    public override void Configure()
    {
        Put("/api/investments");
    }

    public override async Task HandleAsync(InvestmentForUpdateDto req, CancellationToken ct)
    {
        var investmentFromDb = await context.Investments.FindAsync([req.InvestmentId], ct);
        context.Entry(investmentFromDb!).State = EntityState.Detached;
        var lastPrice = investmentFromDb!.LastPrice;
        var lastPriceRetrievalDate = investmentFromDb.LastPriceRetrievalDate;

        var investmentMapped = req.ToInvestment();
        investmentMapped.LastPrice = lastPrice;
        investmentMapped.LastPriceRetrievalDate = lastPriceRetrievalDate;
        context.Entry(investmentMapped).State = EntityState.Modified;
        if (investmentMapped.CategoryId == Guid.Empty)
        {
            investmentMapped.CategoryId = null;
        }

        await context.SaveChangesAsync(ct);
        await context.Entry(investmentMapped).ReloadAsync(ct);
        await context.Entry(investmentMapped).Collection(i => i.Transactions).LoadAsync(ct);
        var returnInvestment = investmentMapped.ToModel();
        await SendOkAsync(returnInvestment, ct);
    }
}