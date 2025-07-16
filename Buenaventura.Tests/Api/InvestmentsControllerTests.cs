using AutoMapper;
using Buenaventura.Api;
using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Buenaventura.Tests.Helpers;
using Buenaventura.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Buenaventura.Tests.Api;

public class InvestmentsControllerTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;
    private readonly Mock<IInvestmentService> _mockInvestmentService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly InvestmentsController _controller;

    public InvestmentsControllerTests(TestDbContextFixture fixture)
    {
        _fixture = fixture;
        _mockInvestmentService = new Mock<IInvestmentService>();
        _mockMapper = new Mock<IMapper>();
        _controller = new InvestmentsController(_fixture.Context, _mockInvestmentService.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetInvestments_ReturnsInvestmentList()
    {
        // Arrange
        var expectedInvestments = new InvestmentListModel
        {
            Investments = TestDataFactory.InvestmentModelFaker.Generate(5),
            PortfolioIrr = 0.08
        };
        
        _mockInvestmentService.Setup(s => s.GetInvestments())
            .ReturnsAsync(expectedInvestments);
        
        // Act
        var result = await _controller.GetInvestments();
        
        // Assert
        result.Should().Be(expectedInvestments);
        _mockInvestmentService.Verify(s => s.GetInvestments(), Times.Once);
    }

    [Fact]
    public async Task UpdateCurrentPrices_ReturnsUpdatedInvestments()
    {
        // Arrange
        var expectedInvestments = new InvestmentListModel
        {
            Investments = TestDataFactory.InvestmentModelFaker.Generate(3),
            PortfolioIrr = 0.12
        };
        
        _mockInvestmentService.Setup(s => s.UpdateCurrentPrices())
            .ReturnsAsync(expectedInvestments);
        
        // Act
        var result = await _controller.UpdateCurrentPrices();
        
        // Assert
        result.Should().Be(expectedInvestments);
        _mockInvestmentService.Verify(s => s.UpdateCurrentPrices(), Times.Once);
    }

    [Fact]
    public async Task RecordDividend_CallsInvestmentService()
    {
        // Arrange
        var investmentId = Guid.NewGuid();
        var dividendModel = new RecordDividendModel
        {
            AccountId = Guid.NewGuid(),
            Amount = 100m,
            Date = DateTime.Now,
            Description = "Test dividend",
            IncomeTax = 10m,
            Total = 90m
        };
        
        // Act
        await _controller.RecordDividend(investmentId, dividendModel);
        
        // Assert
        _mockInvestmentService.Verify(s => s.RecordDividend(investmentId, dividendModel), Times.Once);
    }

    [Fact]
    public async Task BuySell_CallsInvestmentService()
    {
        // Arrange
        var buySellModel = new BuySellModel
        {
            InvestmentId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Shares = 10,
            Price = 50m,
            Date = DateTime.Now
        };
        
        // Act
        await _controller.BuySell(buySellModel);
        
        // Assert
        _mockInvestmentService.Verify(s => s.BuySell(buySellModel), Times.Once);
    }

    [Fact]
    public async Task SaveTodaysPrices_UpdatesInvestmentPrices()
    {
        // Arrange
        var investments = TestDataFactory.InvestmentFaker.Generate(3);
        _fixture.Context.Investments.AddRange(investments);
        await _fixture.Context.SaveChangesAsync();
        
        var pricesDto = investments.Select(i => new TodaysPriceDto
        {
            InvestmentId = i.InvestmentId,
            LastPrice = i.LastPrice + 10m // New price
        }).ToList();
        
        var expectedReturn = new InvestmentListModel
        {
            Investments = new List<InvestmentModel>(),
            PortfolioIrr = 0.10
        };
        
        _mockInvestmentService.Setup(s => s.GetInvestments())
            .ReturnsAsync(expectedReturn);
        
        // Act
        var result = await _controller.SaveTodaysPrices(pricesDto);
        
        // Assert
        result.Should().Be(expectedReturn);
        
        // Verify prices were updated
        var updatedInvestments = await _fixture.Context.Investments
            .Where(i => investments.Select(inv => inv.InvestmentId).Contains(i.InvestmentId))
            .ToListAsync();
        
        updatedInvestments.Should().AllSatisfy(i => 
        {
            i.LastPriceRetrievalDate.Should().Be(DateTime.Today);
            var expectedPrice = pricesDto.First(p => p.InvestmentId == i.InvestmentId).LastPrice;
            i.LastPrice.Should().Be(expectedPrice);
        });
    }

    [Fact]
    public async Task DeleteInvestment_RemovesInvestment()
    {
        // Arrange
        var investment = TestDataFactory.InvestmentFaker.Generate();
        _fixture.Context.Investments.Add(investment);
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        await _controller.Delete(investment.InvestmentId);
        
        // Assert
        var deletedInvestment = await _fixture.Context.Investments.FindAsync(investment.InvestmentId);
        deletedInvestment.Should().BeNull();
    }

    [Fact]
    public async Task GetInvestment_ExistingInvestment_ReturnsInvestment()
    {
        // Arrange
        var investment = TestDataFactory.InvestmentFaker.Generate();
        _fixture.Context.Investments.Add(investment);
        await _fixture.Context.SaveChangesAsync();
        
        var expectedModel = TestDataFactory.InvestmentModelFaker.Generate();
        expectedModel.InvestmentId = investment.InvestmentId;
        
        _mockMapper.Setup(m => m.Map<InvestmentModel>(It.IsAny<Investment>()))
            .Returns(expectedModel);
        
        // Act
        var result = await _controller.Get(investment.InvestmentId);
        
        // Assert
        result.Should().Be(expectedModel);
        _mockMapper.Verify(m => m.Map<InvestmentModel>(It.IsAny<Investment>()), Times.Once);
    }

    [Fact]
    public async Task GetInvestment_NonExistentInvestment_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        // Act
        var result = await _controller.Get(nonExistentId);
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddInvestment_ValidInvestment_AddsInvestment()
    {
        // Arrange
        var investmentModel = new AddInvestmentModel
        {
            Name = "Test Investment",
            Symbol = "TEST",
            Currency = "USD",
            CategoryId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Shares = 100,
            Price = 50m,
            Date = DateTime.Now
        };
        
        // Act
        await _controller.PostInvestment(investmentModel);
        
        // Assert
        _mockInvestmentService.Verify(s => s.AddInvestment(investmentModel), Times.Once);
    }
}
