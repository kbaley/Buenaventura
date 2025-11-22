using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Api;

internal record GetInvestmentRequest(Guid InvestmentId);

internal class GetInvestment(BuenaventuraDbContext context)
    : Endpoint<GetInvestmentRequest, InvestmentDetailDto>
{
    public override void Configure()
    {
        Get("/api/investments/{InvestmentId}");
    }

    public override async Task HandleAsync(GetInvestmentRequest req, CancellationToken ct)
    {
        var investment = await context.Investments
            .Include(i => i.Dividends)
            .Include(i => i.Transactions)
            .ThenInclude(t => t.Transaction.Account)
            .SingleOrDefaultAsync(i => i.InvestmentId == req.InvestmentId, ct);
        if (investment is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await context.Entry(investment).Collection(i => i.Transactions).LoadAsync(ct);
        var dividends = GetDividendDtosFrom(investment).ToList();
        var mappedInvestment = investment.ToDto();
        mappedInvestment.Transactions = mappedInvestment.Transactions.OrderBy(t => t.Date);
        mappedInvestment.TotalPaid = decimal.Round(investment.Transactions.Sum(t => t.Shares * t.Price), 2);
        mappedInvestment.CurrentValue = decimal.Round(mappedInvestment.LastPrice * mappedInvestment.Shares, 2);
        mappedInvestment.BookValue = decimal.Round(mappedInvestment.AveragePrice * mappedInvestment.Shares, 2);
        mappedInvestment.Dividends = dividends;

        await SendOkAsync(mappedInvestment, ct);
    }

    private static IEnumerable<RecordDividendModel> GetDividendDtosFrom(Investment investment)
    {
        var dividendTransactions = investment.Dividends
            .OrderBy(d => d.TransactionDate)
            .ThenBy(d => d.EnteredDate)
            .ThenBy(d => d.Amount)
            .ToList();
        var dividends = new List<RecordDividendModel>();
        var i = 0;
        while (i < dividendTransactions.Count)
        {
            var dividend = new RecordDividendModel
            {
                Date = dividendTransactions[i].TransactionDate,
            };
            if (dividendTransactions[i].Amount < 0)
            {
                dividend.IncomeTax = -dividendTransactions[i++].Amount;
            }

            dividend.Amount = dividendTransactions[i++].Amount;
            dividend.Total = dividend.Amount - dividend.IncomeTax;

            dividends.Add(dividend);
        }

        return dividends;
    }
}