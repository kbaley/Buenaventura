using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Services;
using Buenaventura.Shared;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Buenaventura.Tests.Services;

public class ServerAccountServiceTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;
    private readonly Mock<ITransactionRepository> _mockTransactionRepo;
    private readonly ServerAccountService _service;

    public ServerAccountServiceTests(TestDbContextFixture fixture)
    {
        _fixture = fixture;
        _mockTransactionRepo = new Mock<ITransactionRepository>();
        _service = new ServerAccountService(_fixture.Context, _mockTransactionRepo.Object);
    }

    [Fact]
    public async Task GetAccounts_ReturnsAccountsWithBalances()
    {
        // Arrange
        var accounts = TestDataFactory.AccountFaker.Generate(3);
        
        _fixture.Context.Accounts.AddRange(accounts);
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        var result = await _service.GetAccounts();
        
        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(a => a.AccountId.Should().NotBeEmpty());
    }

    [Fact]
    public async Task GetAccount_ExistingAccount_ReturnsAccount()
    {
        // Arrange
        var account = TestDataFactory.AccountFaker.Generate();
        _fixture.Context.Accounts.Add(account);
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        var result = await _service.GetAccount(account.AccountId);
        
        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(account.AccountId);
        result.Name.Should().Be(account.Name);
    }

    [Fact]
    public async Task GetAccount_NonExistentAccount_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        // Act
        var result = await _service.GetAccount(nonExistentId);
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTransactions_CallsTransactionRepository()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var expectedResult = new TransactionListModel
        {
            Items = new List<TransactionForDisplay>(),
            StartingBalance = 1000m,
            TotalCount = 0
        };
        
        _mockTransactionRepo.Setup(r => r.GetByAccount(accountId, "", 0, 50))
            .ReturnsAsync(expectedResult);
        
        // Act
        var result = await _service.GetTransactions(accountId);
        
        // Assert
        result.Should().Be(expectedResult);
        _mockTransactionRepo.Verify(r => r.GetByAccount(accountId, "", 0, 50), Times.Once);
    }

    [Fact]
    public async Task UpdateAccount_ExistingAccount_UpdatesProperties()
    {
        // Arrange
        var account = TestDataFactory.AccountFaker.Generate();
        _fixture.Context.Accounts.Add(account);
        await _fixture.Context.SaveChangesAsync();
        
        var updatedAccount = new AccountWithBalance
        {
            AccountId = account.AccountId,
            Name = "Updated Name",
            Currency = "EUR",
            Vendor = "Updated Vendor",
            AccountType = "Updated Type",
            IsHidden = !account.IsHidden
        };
        
        // Act
        await _service.UpdateAccount(updatedAccount);
        
        // Assert
        var dbAccount = await _fixture.Context.Accounts.FindAsync(account.AccountId);
        dbAccount.Should().NotBeNull();
        dbAccount!.Name.Should().Be("Updated Name");
        dbAccount.Currency.Should().Be("EUR");
        dbAccount.Vendor.Should().Be("Updated Vendor");
        dbAccount.AccountType.Should().Be("Updated Type");
        dbAccount.IsHidden.Should().Be(updatedAccount.IsHidden);
    }

    [Fact]
    public async Task UpdateAccount_NonExistentAccount_DoesNotThrow()
    {
        // Arrange
        var nonExistentAccount = new AccountWithBalance
        {
            AccountId = Guid.NewGuid(),
            Name = "Test"
        };
        
        // Act &amp; Assert
        await FluentActions.Invoking(() => _service.UpdateAccount(nonExistentAccount))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveAccountOrder_UpdatesDisplayOrder()
    {
        // Arrange
        var accounts = TestDataFactory.AccountFaker.Generate(3);
        _fixture.Context.Accounts.AddRange(accounts);
        await _fixture.Context.SaveChangesAsync();
        
        var accountOrders = accounts.Select((a, i) => new OrderedAccount
        {
            AccountId = a.AccountId,
            DisplayOrder = i + 10
        }).ToList();
        
        // Act
        await _service.SaveAccountOrder(accountOrders);
        
        // Assert
        var updatedAccounts = await _fixture.Context.Accounts
            .Where(a => accounts.Select(acc => acc.AccountId).Contains(a.AccountId))
            .ToListAsync();
        
        updatedAccounts.Should().HaveCount(3);
        updatedAccounts.Should().AllSatisfy(a => a.DisplayOrder.Should().BeGreaterOrEqualTo(10));
    }

    [Fact]
    public async Task DeleteTransaction_CallsTransactionRepository()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.TransactionId = transactionId;
        
        _mockTransactionRepo.Setup(r => r.Get(transactionId))
            .ReturnsAsync(new TransactionForDisplay { TransactionId = transactionId });
        
        // Act
        await _service.DeleteTransaction(transactionId);
        
        // Assert
        _mockTransactionRepo.Verify(r => r.Get(transactionId), Times.Once);
        _mockTransactionRepo.Verify(r => r.Delete(transactionId), Times.Once);
    }

    [Fact]
    public async Task AddTransaction_CallsTransactionRepository()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transaction = TestDataFactory.TransactionForDisplayFaker.Generate();
        transaction.Debit = 100m;  // Ensure we have a debit value
        
        // Act
        await _service.AddTransaction(accountId, transaction);
        
        // Assert
        _mockTransactionRepo.Verify(r => r.Insert(transaction), Times.Once);
        transaction.AccountId.Should().Be(accountId);
    }

    [Fact]
    public async Task UpdateTransaction_CallsTransactionRepository()
    {
        // Arrange
        var transaction = TestDataFactory.TransactionForDisplayFaker.Generate();
        transaction.Credit = 150m;  // Ensure we have a credit value
        
        // Act
        await _service.UpdateTransaction(transaction);
        
        // Assert
        _mockTransactionRepo.Verify(r => r.Update(transaction), Times.Once);
    }
}
