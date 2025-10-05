using Buenaventura.Domain;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Buenaventura.Tests.Domain;

public class InvestmentTests
{
    [Fact]
    public void GetTotalReturn_WithTransactionsAndDividends_CalculatesCorrectly()
    {
        // Arrange
        var investment = TestDataFactory.InvestmentFaker.Generate();
        investment.LastPrice = 50m;

        var transactions = new List<InvestmentTransaction>
        {
            new() { Shares = 10, Price = 40m }, // $400 investment
            new() { Shares = 5, Price = 60m }   // $300 investment
        };
        investment.Transactions = transactions;

        var dividends = new List<Transaction>
        {
            new() { Amount = 25m },
            new() { Amount = 15m }
        };
        investment.Dividends = dividends;

        // Act
        var result = investment.GetTotalReturn();

        // Assert
        // Total paid: $700, Current value: 15 shares * $50 = $750, Dividends: $40
        // Total return: ($750 + $40 - $700) / $750 = $90 / $750 = 0.12
        result.Should().BeApproximately(0.12m, 0.01m);
    }

    [Fact]
    public void GetTotalReturn_WithNoTransactions_ReturnsZero()
    {
        // Arrange
        var investment = TestDataFactory.InvestmentFaker.Generate();
        investment.Transactions = new List<InvestmentTransaction>();
        investment.Dividends = new List<Transaction>();

        // Act
        var result = investment.GetTotalReturn();

        // Assert
        result.Should().Be(0m);
    }

    [Fact]
    public void GetCurrentValue_WithTransactions_CalculatesCorrectly()
    {
        // Arrange
        var investment = TestDataFactory.InvestmentFaker.Generate();
        investment.LastPrice = 50m;

        var transactions = new List<InvestmentTransaction>
        {
            new() { Shares = 10, Price = 40m },
            new() { Shares = 5, Price = 60m },
            new() { Shares = -2, Price = 55m } // Sell 2 shares
        };
        investment.Transactions = transactions;

        // Act
        var result = investment.GetCurrentValue();

        // Assert
        // Total shares: 10 + 5 - 2 = 13 shares
        // Current value: 13 * $50 = $650
        result.Should().Be(650m);
    }

    [Fact]
    public void GetCurrentValue_WithNoTransactions_ReturnsZero()
    {
        // Arrange
        var investment = TestDataFactory.InvestmentFaker.Generate();
        investment.Transactions = new List<InvestmentTransaction>();

        // Act
        var result = investment.GetCurrentValue();

        // Assert
        result.Should().Be(0m);
    }

    [Fact]
    public void GetAnnualizedIrr_WithNoTransactions_ReturnsZero()
    {
        // Arrange
        var investment = TestDataFactory.InvestmentFaker.Generate();
        investment.Transactions = new List<InvestmentTransaction>();

        // Act
        var result = investment.GetAnnualizedIrr();

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void GetAnnualizedIrr_WithTransactions_CalculatesIrr()
    {
        // Arrange
        var investment = TestDataFactory.InvestmentFaker.Generate();
        investment.LastPrice = 60m;

        var baseDate = new DateTime(2024, 1, 1);
        var transactions = new List<InvestmentTransaction>
        {
            new() { Shares = 10, Price = 50m, Date = baseDate },
            new() { Shares = 5, Price = 55m, Date = baseDate.AddDays(90) }
        };
        investment.Transactions = transactions;

        // Act
        var result = investment.GetAnnualizedIrr();

        // Assert
        // Should return a positive IRR since current value is higher than investment
        result.Should().BeGreaterThan(0.0);
    }
}