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
