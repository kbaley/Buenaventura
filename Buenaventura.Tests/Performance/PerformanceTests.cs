using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Services;
using Buenaventura.Shared;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Xunit;

namespace Buenaventura.Tests.Performance;

public class TransactionRepositoryPerformanceTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;
    private readonly TransactionRepository _repository;

    public TransactionRepositoryPerformanceTests(TestDbContextFixture fixture)
    {
        _fixture = fixture;
        _repository = new TransactionRepository(_fixture.Context);
    }

    [Fact]
    public async Task GetByAccount_WithLargeDataset_PerformsWithinTimeLimit()
    {
        // Arrange
        var account = TestDataFactory.AccountFaker.Generate();
        _fixture.Context.Accounts.Add(account);
        await _fixture.Context.SaveChangesAsync();

        var transactions = TestDataFactory.TransactionFaker.Generate(10000);
        transactions.ForEach(t => t.AccountId = account.AccountId);

        // Add transactions in batches to avoid memory issues
        var batchSize = 1000;
        for (int i = 0; i < transactions.Count; i += batchSize)
        {
            var batch = transactions.Skip(i).Take(batchSize).ToList();
            _fixture.Context.Transactions.AddRange(batch);
            await _fixture.Context.SaveChangesAsync();
        }

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _repository.GetByAccount(account.AccountId);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(50);
        result.TotalCount.Should().Be(10000);

        // Performance assertion - should complete within 2 seconds
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    [Fact]
    public async Task GetByAccount_WithPaginationAndSearch_PerformsWithinTimeLimit()
    {
        // Arrange
        var account = TestDataFactory.AccountFaker.Generate();
        _fixture.Context.Accounts.Add(account);
        await _fixture.Context.SaveChangesAsync();

        var transactions = TestDataFactory.TransactionFaker.Generate(5000);
        transactions.ForEach(t => t.AccountId = account.AccountId);

        // Add specific vendors for search testing
        for (int i = 0; i < 100; i++)
        {
            transactions[i].Vendor = $"SearchVendor_{i}";
        }

        _fixture.Context.Transactions.AddRange(transactions);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _repository.GetByAccount(account.AccountId, "SearchVendor", 0, 25);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(25);
        result.Items.Should().AllSatisfy(t => t.Vendor.Should().Contain("SearchVendor"));

        // Performance assertion - should complete within 1 second
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    public async Task InsertTransactions_InBulk_PerformsWithinTimeLimit()
    {
        // Arrange
        var account = TestDataFactory.AccountFaker.Generate();
        _fixture.Context.Accounts.Add(account);

        // Add some basic categories for the transactions
        var categories = new List<Category>
        {
            new Category { CategoryId = Guid.NewGuid(), Name = "Test Expense", Type = "Expense" },
            new Category { CategoryId = Guid.NewGuid(), Name = "Test Income", Type = "Income" },
            new Category { CategoryId = Guid.NewGuid(), Name = "Test Transfer", Type = "Transfer" }
        };
        _fixture.Context.Categories.AddRange(categories);
        await _fixture.Context.SaveChangesAsync();

        var transactions = TestDataFactory.TransactionForDisplayFaker.Generate(1000);
        transactions.ForEach(t => 
        {
            t.AccountId = account.AccountId;
            // Use one of the existing categories
            var category = categories[Random.Shared.Next(categories.Count)];
            t.Category = new CategoryModel 
            { 
                CategoryId = category.CategoryId, 
                Name = category.Name,
                CategoryClass = category.Type
            };
        });

        // Act
        var stopwatch = Stopwatch.StartNew();

        foreach (var transaction in transactions)
        {
            await _repository.Insert(transaction);
        }

        stopwatch.Stop();

        // Assert
        var insertedCount = await _fixture.Context.Transactions.CountAsync(t => t.AccountId == account.AccountId);
        insertedCount.Should().Be(1000);

        // Performance assertion - should complete within 15 seconds (increased for CI environments)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000);
    }
}

public class AccountServicePerformanceTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;
    private readonly AccountService _service;
    private readonly TransactionRepository _repository;

    public AccountServicePerformanceTests(TestDbContextFixture fixture)
    {
        _fixture = fixture;
        _repository = new TransactionRepository(_fixture.Context);
        _service = new AccountService(_fixture.Context, _repository);
    }

    [Fact]
    public async Task GetAccounts_WithManyAccounts_PerformsWithinTimeLimit()
    {
        // Arrange
        // Clear any existing accounts
        _fixture.Context.Accounts.RemoveRange(_fixture.Context.Accounts);
        await _fixture.Context.SaveChangesAsync();

        var accounts = TestDataFactory.AccountFaker.Generate(1000);
        // Ensure all accounts are visible
        foreach (var account in accounts)
        {
            account.IsHidden = false;
        }

        _fixture.Context.Accounts.AddRange(accounts);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _service.GetAccounts();
        stopwatch.Stop();

        // Assert
        result.Should().HaveCount(1000);

        // Performance assertion - should complete within 1 second
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    public async Task GetTransactions_WithManyTransactions_PerformsWithinTimeLimit()
    {
        // Arrange
        var account = TestDataFactory.AccountFaker.Generate();
        _fixture.Context.Accounts.Add(account);
        await _fixture.Context.SaveChangesAsync();

        var transactions = TestDataFactory.TransactionFaker.Generate(5000);
        transactions.ForEach(t => t.AccountId = account.AccountId);
        _fixture.Context.Transactions.AddRange(transactions);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _service.GetTransactions(account.AccountId);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(50); // Default page size
        result.TotalCount.Should().Be(5000);

        // Performance assertion - should complete within 1 second
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }
}