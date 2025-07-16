using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Buenaventura.Tests.Data;

public class TransactionRepositoryTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;
    private readonly TransactionRepository _repository;

    public TransactionRepositoryTests(TestDbContextFixture fixture)
    {
        _fixture = fixture;
        _repository = new TransactionRepository(_fixture.Context);
    }

    [Fact]
    public async Task GetByAccount_ReturnsTransactionsForAccount()
    {
        // Arrange
        var account = TestDataFactory.AccountFaker.Generate();
        _fixture.Context.Accounts.Add(account);
        
        var transactions = TestDataFactory.TransactionFaker.Generate(5);
        transactions.ForEach(t => t.AccountId = account.AccountId);
        _fixture.Context.Transactions.AddRange(transactions);
        
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetByAccount(account.AccountId);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.Items.Should().AllSatisfy(t => t.AccountId.Should().Be(account.AccountId));
    }

    [Fact]
    public async Task GetByAccount_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        var account = TestDataFactory.AccountFaker.Generate();
        _fixture.Context.Accounts.Add(account);
        
        var transactions = TestDataFactory.TransactionFaker.Generate(20);
        transactions.ForEach(t => t.AccountId = account.AccountId);
        _fixture.Context.Transactions.AddRange(transactions);
        
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetByAccount(account.AccountId, "", 0, 10);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(20);
    }

    [Fact]
    public async Task GetByAccount_WithSearch_FiltersResults()
    {
        // Arrange
        var account = TestDataFactory.AccountFaker.Generate();
        _fixture.Context.Accounts.Add(account);
        
        var transactions = TestDataFactory.TransactionFaker.Generate(5);
        transactions.ForEach(t => t.AccountId = account.AccountId);
        transactions[0].Vendor = "Special Vendor";
        transactions[1].Description = "Special Description";
        
        _fixture.Context.Transactions.AddRange(transactions);
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetByAccount(account.AccountId, "Special");
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(t => t.Vendor == "Special Vendor");
        result.Items.Should().Contain(t => t.Description == "Special Description");
    }

    [Fact]
    public async Task GetInDateRange_ReturnsTransactionsInRange()
    {
        // Arrange
        var account = TestDataFactory.AccountFaker.Generate();
        _fixture.Context.Accounts.Add(account);
        
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        
        var transactions = TestDataFactory.TransactionFaker.Generate(10);
        transactions.ForEach(t => t.AccountId = account.AccountId);
        
        // Set 5 transactions within range
        for (int i = 0; i < 5; i++)
        {
            transactions[i].TransactionDate = startDate.AddDays(i * 30);
        }
        
        // Set 5 transactions outside range
        for (int i = 5; i < 10; i++)
        {
            transactions[i].TransactionDate = startDate.AddYears(-1);
        }
        
        _fixture.Context.Transactions.AddRange(transactions);
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetInDateRange(account.AccountId, startDate, endDate);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.Items.Should().AllSatisfy(t => 
            t.TransactionDate.Should().BeOnOrAfter(startDate).And.BeOnOrBefore(endDate));
    }

    [Fact]
    public async Task Get_ExistingTransaction_ReturnsTransaction()
    {
        // Arrange
        var transaction = TestDataFactory.TransactionFaker.Generate();
        _fixture.Context.Transactions.Add(transaction);
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        var result = await _repository.Get(transaction.TransactionId);
        
        // Assert
        result.Should().NotBeNull();
        result.TransactionId.Should().Be(transaction.TransactionId);
        result.Vendor.Should().Be(transaction.Vendor);
        result.Description.Should().Be(transaction.Description);
        result.Amount.Should().Be(transaction.Amount);
    }

    [Fact]
    public async Task Get_NonExistentTransaction_ReturnsEmptyObject()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        // Act
        var result = await _repository.Get(nonExistentId);
        
        // Assert
        result.Should().NotBeNull();
        result.TransactionId.Should().Be(Guid.Empty);
        result.Description.Should().Be("");
    }

    [Fact]
    public async Task Insert_ValidTransaction_AddsTransaction()
    {
        // Arrange
        var transactionDto = TestDataFactory.TransactionForDisplayFaker.Generate();
        transactionDto.TransactionId = Guid.NewGuid();
        
        // Ensure the category exists in the database
        var category = new Category 
        { 
            CategoryId = transactionDto.Category.CategoryId!.Value, 
            Name = transactionDto.Category.Name,
            Type = transactionDto.Category.CategoryClass
        };
        _fixture.Context.Categories.Add(category);
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        await _repository.Insert(transactionDto);
        
        // Assert
        var dbTransaction = await _fixture.Context.Transactions.FindAsync(transactionDto.TransactionId);
        dbTransaction.Should().NotBeNull();
        dbTransaction!.Vendor.Should().Be(transactionDto.Vendor);
        dbTransaction.Description.Should().Be(transactionDto.Description);
        dbTransaction.Amount.Should().Be(transactionDto.Amount);
    }

    [Fact]
    public async Task Update_ExistingTransaction_UpdatesTransaction()
    {
        // Arrange
        var transaction = TestDataFactory.TransactionFaker.Generate();
        _fixture.Context.Transactions.Add(transaction);
        
        // Ensure the category exists in the database
        var category = new Category 
        { 
            CategoryId = transaction.CategoryId ?? Guid.NewGuid(), 
            Name = "Test Category",
            Type = "Expense"
        };
        _fixture.Context.Categories.Add(category);
        await _fixture.Context.SaveChangesAsync();
        
        var updatedTransaction = new TransactionForDisplay
        {
            TransactionId = transaction.TransactionId,
            AccountId = transaction.AccountId,
            Vendor = "Updated Vendor",
            Description = "Updated Description",
            Amount = 999.99m,
            TransactionDate = DateTime.Now,
            Category = new CategoryModel { CategoryId = category.CategoryId, Name = "Test Category", CategoryClass = "Expense" },
            TransactionType = transaction.TransactionType
        };
        
        // Act
        var result = await _repository.Update(updatedTransaction);
        
        // Assert
        result.Should().NotBeNull();
        result.Vendor.Should().Be("Updated Vendor");
        result.Description.Should().Be("Updated Description");
        result.Amount.Should().Be(999.99m);
        
        var dbTransaction = await _fixture.Context.Transactions.FindAsync(transaction.TransactionId);
        dbTransaction.Should().NotBeNull();
        dbTransaction!.Vendor.Should().Be("Updated Vendor");
        dbTransaction.Description.Should().Be("Updated Description");
        dbTransaction.Amount.Should().Be(999.99m);
    }

    [Fact]
    public async Task Delete_ExistingTransaction_RemovesTransaction()
    {
        // Arrange
        var transaction = TestDataFactory.TransactionFaker.Generate();
        _fixture.Context.Transactions.Add(transaction);
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        await _repository.Delete(transaction.TransactionId);
        
        // Assert
        var dbTransaction = await _fixture.Context.Transactions.FindAsync(transaction.TransactionId);
        dbTransaction.Should().BeNull();
    }

    [Fact]
    public async Task Delete_NonExistentTransaction_DoesNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        // Act &amp; Assert
        await FluentActions.Invoking(() => _repository.Delete(nonExistentId))
            .Should().NotThrowAsync();
    }
}
