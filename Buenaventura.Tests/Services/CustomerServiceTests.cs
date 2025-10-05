using Buenaventura.Services;
using Buenaventura.Shared;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Buenaventura.Tests.Services;

public class CustomerServiceTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;
    private readonly CustomerService _service;

    public CustomerServiceTests(TestDbContextFixture fixture)
    {
        _fixture = fixture;
        _service = new CustomerService(_fixture.Context);
    }

    [Fact]
    public async Task GetCustomers_ReturnsAllCustomers()
    {
        // Arrange
        _fixture.Context.Customers.RemoveRange(_fixture.Context.Customers);
        var customers = TestDataFactory.CustomerFaker.Generate(5);
        _fixture.Context.Customers.AddRange(customers);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = (await _service.GetCustomers()).ToList();

        // Assert
        result.Should().HaveCount(5);
        result.Should().AllSatisfy(c => c.CustomerId.Should().NotBeEmpty());
        result.Should().BeInAscendingOrder(c => c.Name);
    }

    [Fact]
    public async Task GetCustomer_ExistingCustomer_ReturnsCustomer()
    {
        // Arrange
        var customer = TestDataFactory.CustomerFaker.Generate();
        _fixture.Context.Customers.Add(customer);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _service.GetCustomer(customer.CustomerId);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customer.CustomerId);
        result.Name.Should().Be(customer.Name);
        result.Email.Should().Be(customer.Email);
        result.ContactName.Should().Be(customer.ContactName);
        result.Address.Should().Be(customer.Address);
        result.City.Should().Be(customer.City);
        result.StreetAddress.Should().Be(customer.StreetAddress);
        result.Region.Should().Be(customer.Region);
    }

    [Fact]
    public async Task GetCustomer_NonExistentCustomer_ThrowsException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act &amp; Assert
        await FluentActions.Invoking(() => _service.GetCustomer(nonExistentId))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Customer not found");
    }

    [Fact]
    public async Task AddCustomer_ValidCustomer_AddsCustomer()
    {
        // Arrange
        var customerModel = TestDataFactory.CustomerModelFaker.Generate();
        var initialCount = await _fixture.Context.Customers.CountAsync();

        // Act
        await _service.AddCustomer(customerModel);

        // Assert
        var finalCount = await _fixture.Context.Customers.CountAsync();
        finalCount.Should().Be(initialCount + 1);

        var dbCustomer = await _fixture.Context.Customers
            .FirstOrDefaultAsync(c => c.Name == customerModel.Name && c.Email == customerModel.Email);
        dbCustomer.Should().NotBeNull();
        dbCustomer.Name.Should().Be(customerModel.Name);
        dbCustomer.Email.Should().Be(customerModel.Email);
        dbCustomer.ContactName.Should().Be(customerModel.ContactName);
    }

    [Fact]
    public async Task UpdateCustomer_ExistingCustomer_UpdatesProperties()
    {
        // Arrange
        var customer = TestDataFactory.CustomerFaker.Generate();
        _fixture.Context.Customers.Add(customer);
        await _fixture.Context.SaveChangesAsync();

        var updatedCustomer = new CustomerModel
        {
            CustomerId = customer.CustomerId,
            Name = "Updated Name",
            Email = "updated@email.com",
            ContactName = "Updated Contact",
            Address = "Updated Address",
            City = "Updated City",
            StreetAddress = "Updated Street",
            Region = "Updated Region"
        };

        // Act
        await _service.UpdateCustomer(updatedCustomer);

        // Assert
        var dbCustomer = await _fixture.Context.Customers.FindAsync(customer.CustomerId);
        dbCustomer.Should().NotBeNull();
        dbCustomer.Name.Should().Be("Updated Name");
        dbCustomer.Email.Should().Be("updated@email.com");
        dbCustomer.ContactName.Should().Be("Updated Contact");
        dbCustomer.Address.Should().Be("Updated Street\nUpdated City, Updated Region");
        dbCustomer.City.Should().Be("Updated City");
        dbCustomer.StreetAddress.Should().Be("Updated Street");
        dbCustomer.Region.Should().Be("Updated Region");
    }

    [Fact]
    public async Task UpdateCustomer_NonExistentCustomer_ThrowsException()
    {
        // Arrange
        var nonExistentCustomer = new CustomerModel
        {
            CustomerId = Guid.NewGuid(),
            Name = "Test"
        };

        // Act &amp; Assert
        await FluentActions.Invoking(() => _service.UpdateCustomer(nonExistentCustomer))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Customer not found");
    }

    [Fact]
    public async Task DeleteCustomer_ExistingCustomer_RemovesCustomer()
    {
        // Arrange
        var customer = TestDataFactory.CustomerFaker.Generate();
        _fixture.Context.Customers.Add(customer);
        await _fixture.Context.SaveChangesAsync();

        // Act
        await _service.DeleteCustomer(customer.CustomerId);

        // Assert
        var dbCustomer = await _fixture.Context.Customers.FindAsync(customer.CustomerId);
        dbCustomer.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCustomer_NonExistentCustomer_ThrowsException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await FluentActions.Invoking(() => _service.DeleteCustomer(nonExistentId))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Customer not found");
    }
}