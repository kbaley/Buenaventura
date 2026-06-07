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
    public async Task GetVendors_WithPaging_ReturnsRequestedPage()
    {
        var category = CreateCategory("Paged Category");
        var vendors = new[]
        {
            CreateVendor("000 Paging A", category.CategoryId),
            CreateVendor("000 Paging B", category.CategoryId),
            CreateVendor("000 Paging C", category.CategoryId)
        };

        _fixture.Context.Categories.Add(category);
        _fixture.Context.Vendors.AddRange(vendors);
        await _fixture.Context.SaveChangesAsync();

        var result = await _service.GetVendors(0, 2, "name", false);

        result.TotalCount.Should().BeGreaterThanOrEqualTo(3);
        result.Items.Select(v => v.Name).Should().ContainInOrder("000 Paging A", "000 Paging B");
    }

    [Fact]
    public async Task GetVendors_SortsByLastPosted()
    {
        var category = CreateCategory("Sorted Category");
        var oldVendor = CreateVendor("Sort Old", category.CategoryId);
        var newVendor = CreateVendor("Sort New", category.CategoryId);

        _fixture.Context.Categories.Add(category);
        _fixture.Context.Vendors.AddRange(oldVendor, newVendor);
        _fixture.Context.Transactions.AddRange(
            CreateTransaction(oldVendor.Name, category.CategoryId, new DateTime(2026, 4, 1)),
            CreateTransaction(newVendor.Name, category.CategoryId, new DateTime(2026, 5, 1)));
        await _fixture.Context.SaveChangesAsync();

        var result = await _service.GetVendors(0, 10, "lastPosted", true);

        result.Items
            .Where(v => v.Name.StartsWith("Sort ", StringComparison.Ordinal))
            .Select(v => v.Name)
            .Should()
            .ContainInOrder("Sort New", "Sort Old");
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

    [Fact]
    public async Task DeleteVendorsLastPostedBefore_RemovesOnlyVendorsOlderThanDate()
    {
        var category = CreateCategory("Bulk Delete Category");
        var oldVendor = CreateVendor("Bulk Delete Old", category.CategoryId);
        var newerVendor = CreateVendor("Bulk Delete Newer", category.CategoryId);
        var noTransactionsVendor = CreateVendor("Bulk Delete No Transactions", category.CategoryId);

        _fixture.Context.Categories.Add(category);
        _fixture.Context.Vendors.AddRange(oldVendor, newerVendor, noTransactionsVendor);
        _fixture.Context.Transactions.AddRange(
            CreateTransaction(oldVendor.Name, category.CategoryId, new DateTime(2025, 12, 31)),
            CreateTransaction(newerVendor.Name, category.CategoryId, new DateTime(2026, 1, 2)));
        await _fixture.Context.SaveChangesAsync();

        var deletedCount = await _service.DeleteVendorsLastPostedBefore(new DateTime(2026, 1, 1));

        deletedCount.Should().Be(1);
        (await _fixture.Context.Vendors.FindAsync(oldVendor.VendorId)).Should().BeNull();
        (await _fixture.Context.Vendors.FindAsync(newerVendor.VendorId)).Should().NotBeNull();
        (await _fixture.Context.Vendors.FindAsync(noTransactionsVendor.VendorId)).Should().NotBeNull();
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

    private static Category CreateCategory(string name)
    {
        return new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = name,
            Type = "Expense"
        };
    }

    private static Vendor CreateVendor(string name, Guid categoryId)
    {
        return new Vendor
        {
            VendorId = Guid.NewGuid(),
            Name = name,
            LastTransactionCategoryId = categoryId
        };
    }
}
