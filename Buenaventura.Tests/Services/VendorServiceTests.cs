using Buenaventura.Domain;
using Buenaventura.Services;
using Buenaventura.Shared;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Buenaventura.Tests.Services;

public class VendorServiceTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;
    private readonly VendorService _service;

    public VendorServiceTests(TestDbContextFixture fixture)
    {
        _fixture = fixture;
        _service = new VendorService(_fixture.Context);
    }

    [Fact]
    public async Task GetVendors_IncludesCategoryNameAndLastTransactionDate()
    {
        var category = new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = "Groceries",
            Type = "Expense"
        };
        var vendor = new Vendor
        {
            VendorId = Guid.NewGuid(),
            Name = "Market",
            LastTransactionCategoryId = category.CategoryId
        };

        _fixture.Context.Categories.Add(category);
        _fixture.Context.Vendors.Add(vendor);
        _fixture.Context.Transactions.AddRange(
            CreateTransaction(vendor.Name, category.CategoryId, new DateTime(2026, 1, 5)),
            CreateTransaction(vendor.Name.ToUpperInvariant(), category.CategoryId, new DateTime(2026, 2, 6)));
        await _fixture.Context.SaveChangesAsync();

        var result = await _service.GetVendors();

        var vendorResult = result.Single(v => v.VendorId == vendor.VendorId);
        vendorResult.CategoryName.Should().Be("Groceries");
        vendorResult.LastTransactionDate.Should().Be(new DateTime(2026, 2, 6));
    }

    [Fact]
    public async Task UpdateVendor_RenamesVendorAndMatchingTransactions()
    {
        var category = new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = "Dining",
            Type = "Expense"
        };
        var vendor = new Vendor
        {
            VendorId = Guid.NewGuid(),
            Name = "Old Cafe",
            LastTransactionCategoryId = category.CategoryId
        };

        _fixture.Context.Categories.Add(category);
        _fixture.Context.Vendors.Add(vendor);
        _fixture.Context.Transactions.Add(CreateTransaction("old cafe", category.CategoryId, new DateTime(2026, 3, 1)));
        await _fixture.Context.SaveChangesAsync();

        await _service.UpdateVendor(new()
        {
            VendorId = vendor.VendorId,
            Name = "New Cafe",
            LastTransactionCategoryId = category.CategoryId
        });

        var dbVendor = await _fixture.Context.Vendors.FindAsync(vendor.VendorId);
        dbVendor!.Name.Should().Be("New Cafe");

        var transaction = await _fixture.Context.Transactions.SingleAsync(t => t.TransactionDate == new DateTime(2026, 3, 1));
        transaction.Vendor.Should().Be("New Cafe");
    }

    [Fact]
    public async Task DeleteVendor_RemovesVendor()
    {
        var vendor = new Vendor
        {
            VendorId = Guid.NewGuid(),
            Name = "Vendor To Delete",
            LastTransactionCategoryId = Guid.NewGuid()
        };

        _fixture.Context.Vendors.Add(vendor);
        await _fixture.Context.SaveChangesAsync();

        await _service.DeleteVendor(vendor.VendorId);

        var dbVendor = await _fixture.Context.Vendors.FindAsync(vendor.VendorId);
        dbVendor.Should().BeNull();
    }

    private static Transaction CreateTransaction(string vendor, Guid categoryId, DateTime date)
    {
        return new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Vendor = vendor,
            CategoryId = categoryId,
            Amount = -10,
            AmountInBaseCurrency = -10,
            TransactionDate = date,
            TransactionType = TransactionType.REGULAR
        };
    }
}
