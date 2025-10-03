using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Dtos;
using Buenaventura.Shared;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

internal class PutInvestment(BuenaventuraDbContext context, AutoMapper.IMapper mapper)
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

        var investmentMapped = mapper.Map<Investment>(req);
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
        var returnInvestment = mapper.Map<InvestmentModel>(investmentMapped);
        await SendOkAsync(returnInvestment, ct);
    }
}