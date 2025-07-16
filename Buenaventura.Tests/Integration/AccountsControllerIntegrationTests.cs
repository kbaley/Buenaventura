using Buenaventura.Data;
using Buenaventura.Shared;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Buenaventura.Tests.Integration;

public class AccountsControllerIntegrationTests : IClassFixture<BuenaventuraWebApplicationFactory<Program>>
{
    private readonly BuenaventuraWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AccountsControllerIntegrationTests(BuenaventuraWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAccounts_ReturnsListOfAccounts()
    {
        // Act
        var response = await _client.GetAsync("/api/accounts");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        // Log the response content for debugging
        Console.WriteLine($"Response Content: {content}");
        
        content.Should().NotBeNullOrEmpty();
        
        var accounts = JsonSerializer.Deserialize<List<AccountWithBalance>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        accounts.Should().NotBeNull();
        accounts.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetAccount_WithValidId_ReturnsAccount()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BuenaventuraDbContext>();
        var account = context.Accounts.First();
        
        // Act
        var response = await _client.GetAsync($"/api/accounts/{account.AccountId}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var accountResult = JsonSerializer.Deserialize<AccountWithBalance>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        accountResult.Should().NotBeNull();
        accountResult!.AccountId.Should().Be(account.AccountId);
        accountResult.Name.Should().Be(account.Name);
    }

    [Fact]
    public async Task GetAccount_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        
        // Act
        var response = await _client.GetAsync($"/api/accounts/{invalidId}");
        
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTransactions_WithValidAccountId_ReturnsTransactions()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BuenaventuraDbContext>();
        var account = context.Accounts.First();
        
        // Add some test transactions
        var transactions = TestDataFactory.TransactionFaker.Generate(5);
        transactions.ForEach(t => t.AccountId = account.AccountId);
        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();
        
        // Act
        var response = await _client.GetAsync($"/api/accounts/{account.AccountId}/transactions");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var transactionList = JsonSerializer.Deserialize<TransactionListModel>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        transactionList.Should().NotBeNull();
        transactionList!.Items.Should().HaveCount(5);
        transactionList.Items.Should().AllSatisfy(t => t.AccountId.Should().Be(account.AccountId));
    }

    [Fact]
    public async Task PostTransaction_WithValidData_CreatesTransaction()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BuenaventuraDbContext>();
        var account = context.Accounts.First();
        var category = context.Categories.First();
        
        var transactionDto = new TransactionForDisplay
        {
            AccountId = account.AccountId,
            Vendor = "Test Vendor",
            Description = "Test Transaction",
            Credit = 100.00m,
            TransactionDate = DateTime.Now,
            Category = new CategoryModel { CategoryId = category.CategoryId, Name = category.Name },
            TransactionType = TransactionType.REGULAR
        };
        
        var json = JsonSerializer.Serialize(transactionDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync($"/api/accounts/{account.AccountId}/transactions", content);
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify the transaction was created
        var createdTransaction = context.Transactions.FirstOrDefault(t => t.Vendor == "Test Vendor");
        createdTransaction.Should().NotBeNull();
        createdTransaction!.Description.Should().Be("Test Transaction");
        createdTransaction.Amount.Should().Be(100.00m);
    }

    [Fact]
    public async Task PutTransaction_WithValidData_UpdatesTransaction()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BuenaventuraDbContext>();
        var account = context.Accounts.First();
        var category = context.Categories.First();
        
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.AccountId = account.AccountId;
        transaction.CategoryId = category.CategoryId;
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        
        var updatedTransaction = new TransactionForDisplay
        {
            TransactionId = transaction.TransactionId,
            AccountId = account.AccountId,
            Vendor = "Updated Vendor",
            Description = "Updated Description",
            Credit = 200.00m,
            TransactionDate = DateTime.Now,
            Category = new CategoryModel { CategoryId = category.CategoryId, Name = category.Name },
            TransactionType = transaction.TransactionType // Keep the same transaction type
        };
        
        var json = JsonSerializer.Serialize(updatedTransaction);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PutAsync($"/api/transactions/{transaction.TransactionId}", content);
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify the transaction was updated
        context.ChangeTracker.Clear(); // Clear the change tracker to get fresh data
        var dbTransaction = await context.Transactions.FindAsync(transaction.TransactionId);
        dbTransaction.Should().NotBeNull();
        dbTransaction!.Vendor.Should().Be("Updated Vendor");
        dbTransaction.Description.Should().Be("Updated Description");
        dbTransaction.Amount.Should().Be(200.00m);
    }

    [Fact]
    public async Task DeleteTransaction_WithValidId_RemovesTransaction()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BuenaventuraDbContext>();
        var account = context.Accounts.First();
        
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.AccountId = account.AccountId;
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        
        // Act
        var response = await _client.DeleteAsync($"/api/transactions/{transaction.TransactionId}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify the transaction was deleted
        context.ChangeTracker.Clear(); // Clear the change tracker to get fresh data
        var deletedTransaction = await context.Transactions.FindAsync(transaction.TransactionId);
        deletedTransaction.Should().BeNull();
    }
}
