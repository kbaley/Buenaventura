using Buenaventura.Data;
using Buenaventura.Services;
using Buenaventura.Shared;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Buenaventura.Tests.Services;

public class CategoryServiceTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;
    private readonly CategoryService _service;

    public CategoryServiceTests(TestDbContextFixture fixture)
    {
        _fixture = fixture;
        var mockTransactionRepo = new Mock<ITransactionRepository>();
        _service = new CategoryService(_fixture.Context, mockTransactionRepo.Object);
    }

    [Fact]
    public async Task GetCategories_ReturnsAllCategories()
    {
        // Arrange
        var categories = TestDataFactory.CategoryFaker.Generate(5);
        _fixture.Context.Categories.AddRange(categories);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = (await _service.GetCategories()).ToList();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(5);
        result.Should().AllSatisfy(c => c.CategoryId.Should().NotBeEmpty());
    }

    [Fact]
    public async Task GetCategories_IncludesTimesUsed()
    {
        // Arrange
        var category = TestDataFactory.CategoryFaker.Generate();
        _fixture.Context.Categories.Add(category);

        var transactions = TestDataFactory.TransactionFaker.Generate(3);
        transactions.ForEach(t => t.CategoryId = category.CategoryId);
        _fixture.Context.Transactions.AddRange(transactions);

        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _service.GetCategories();

        // Assert
        var categoryResult = result.FirstOrDefault(c => c.CategoryId == category.CategoryId);
        categoryResult.Should().NotBeNull();
        categoryResult.TimesUsed.Should().Be(3);
    }

    [Fact]
    public async Task GetCategory_ExistingCategory_ReturnsCategory()
    {
        // Arrange
        var category = TestDataFactory.CategoryFaker.Generate();
        _fixture.Context.Categories.Add(category);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _service.GetCategory(category.CategoryId);

        // Assert
        result.Should().NotBeNull();
        result.CategoryId.Should().Be(category.CategoryId);
        result.Name.Should().Be(category.Name);
        result.CategoryClass.Should().Be(category.Type);
        result.IncludeInReports.Should().Be(category.IncludeInReports);
    }

    [Fact]
    public async Task GetCategory_NonExistentCategory_ThrowsException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act &amp; Assert
        await FluentActions.Invoking(() => _service.GetCategory(nonExistentId))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Category not found");
    }

    [Fact]
    public async Task UpdateCategory_ExistingCategory_UpdatesProperties()
    {
        // Arrange
        var category = TestDataFactory.CategoryFaker.Generate();
        _fixture.Context.Categories.Add(category);
        await _fixture.Context.SaveChangesAsync();

        var updatedCategory = new CategoryModel
        {
            CategoryId = category.CategoryId,
            Name = "Updated Name",
            CategoryClass = "Updated Type",
            IncludeInReports = true
        };

        // Act
        await _service.UpdateCategory(updatedCategory);

        // Assert
        var dbCategory = await _fixture.Context.Categories.FindAsync(category.CategoryId);
        dbCategory.Should().NotBeNull();
        dbCategory.Name.Should().Be("Updated Name");
        dbCategory.Type.Should().Be("Updated Type");
        dbCategory.IncludeInReports.Should().Be(true);
    }

    [Fact]
    public async Task UpdateCategory_NonExistentCategory_ThrowsException()
    {
        // Arrange
        var nonExistentCategory = new CategoryModel
        {
            CategoryId = Guid.NewGuid(),
            Name = "Test"
        };

        // Act &amp; Assert
        await FluentActions.Invoking(() => _service.UpdateCategory(nonExistentCategory))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Category not found");
    }

    [Fact]
    public async Task DeleteCategory_ExistingCategory_RemovesCategory()
    {
        // Arrange
        var category = TestDataFactory.CategoryFaker.Generate();
        _fixture.Context.Categories.Add(category);
        await _fixture.Context.SaveChangesAsync();

        // Act
        await _service.DeleteCategory(category.CategoryId);

        // Assert
        var dbCategory = await _fixture.Context.Categories.FindAsync(category.CategoryId);
        dbCategory.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCategory_NonExistentCategory_DoesNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act &amp; Assert
        await FluentActions.Invoking(() => _service.DeleteCategory(nonExistentId))
            .Should().NotThrowAsync();
    }
}