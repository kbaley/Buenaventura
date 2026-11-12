using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Services;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Buenaventura.Tests.Services;

public class DashboardServiceTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;
    private readonly DashboardService _service;

    public DashboardServiceTests(TestDbContextFixture fixture)
    {
        _fixture = fixture;
        var reportRepo = new Mock<IReportRepository>();
        _service = new DashboardService(_fixture.Context, reportRepo.Object);
    }

    [Fact]
    public async Task GetInvestmentData_AccumulatesGainsAndLossesOnly()
    {
        var period = ReportPeriod.GetLast12Months();
        var reportRepo = new Mock<IReportRepository>(MockBehavior.Strict);
        for (var month = 0; month < 12; month++)
        {
            var start = period.Start.AddMonths(month);
            reportRepo.Setup(repo => repo.GetInvestmentChangeFor(start, start.LastDayOfMonth()))
                .ReturnsAsync(month % 2 == 0 ? 2000m : -500m);
        }
        var service = new DashboardService(_fixture.Context, reportRepo.Object);

        var result = (await service.GetInvestmentData()).ToList();

        result.Select(point => point.Value).Should().Equal(
            2000m, 1500m, 3500m, 3000m, 5000m, 4500m,
            6500m, 6000m, 8000m, 7500m, 9500m, 9000m);
        result.Select(point => point.Label).Should().Equal(Enumerable.Range(0, 12)
            .Select(month => period.Start.AddMonths(month).ToString("MMM yy")));
    }

    [Fact]
    public async Task GetPortfolioValueData_ReturnsMonthlyPortfolioBalancesWithoutAccumulatingThem()
    {
        var period = ReportPeriod.GetLast12Months();
        var dates = Enumerable.Range(0, 12)
            .Select(month => period.Start.AddMonths(month).LastDayOfMonth()).ToList();
        var balances = Enumerable.Range(0, 12)
            .Select(month => 200000m + (month % 2 == 0 ? month * 1000m : -month * 500m)).ToList();
        var reportRepo = new Mock<IReportRepository>(MockBehavior.Strict);
        for (var i = 0; i < dates.Count; i++)
            reportRepo.Setup(repo => repo.GetInvestmentTotalFor(dates[i])).ReturnsAsync(balances[i]);
        var service = new DashboardService(_fixture.Context, reportRepo.Object);

        var result = (await service.GetPortfolioValueData()).ToList();

        result.Select(point => point.Value).Should().Equal(balances);
        result.Select(point => point.Label).Should().Equal(dates.Select(date => date.ToString("MMM yy")));
        foreach (var date in dates)
            reportRepo.Verify(repo => repo.GetInvestmentTotalFor(date), Times.Once);
        reportRepo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetLiquidAssetBalance_UsesCurrentAccountBalanceForCadAccounts()
    {
        _fixture.Context.Transactions.RemoveRange(_fixture.Context.Transactions);
        _fixture.Context.Accounts.RemoveRange(_fixture.Context.Accounts);
        await _fixture.Context.SaveChangesAsync();

        var cadAccount = TestDataFactory.AccountFaker.Generate();
        cadAccount.Currency = "CAD";
        cadAccount.AccountType = "Bank Account";
        cadAccount.IsHidden = false;

        var usdAccount = TestDataFactory.AccountFaker.Generate();
        usdAccount.Currency = "USD";
        usdAccount.AccountType = "Cash";
        usdAccount.IsHidden = false;

        _fixture.Context.Accounts.AddRange(cadAccount, usdAccount);

        _fixture.Context.Transactions.AddRange(
            new Transaction
            {
                TransactionId = Guid.NewGuid(),
                AccountId = cadAccount.AccountId,
                Amount = 100m,
                AmountInBaseCurrency = 75m,
                TransactionDate = DateTime.UtcNow
            },
            new Transaction
            {
                TransactionId = Guid.NewGuid(),
                AccountId = cadAccount.AccountId,
                Amount = -100m,
                AmountInBaseCurrency = -50m,
                TransactionDate = DateTime.UtcNow
            },
            new Transaction
            {
                TransactionId = Guid.NewGuid(),
                AccountId = usdAccount.AccountId,
                Amount = 20m,
                AmountInBaseCurrency = 20m,
                TransactionDate = DateTime.UtcNow
            });

        await _fixture.Context.SaveChangesAsync();

        var result = await _service.GetLiquidAssetBalance();

        result.Should().Be(20m);
    }
}
