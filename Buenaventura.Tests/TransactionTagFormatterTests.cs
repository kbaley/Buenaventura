using Buenaventura.Shared;
using FluentAssertions;
using Xunit;

namespace Buenaventura.Tests;

public class TransactionTagFormatterTests
{
    [Fact]
    public void ParseHashTags_ReturnsIdentifierStyleTags()
    {
        var tags = TransactionTagFormatter.ParseHashTags(
            "Dinner in Lisbon #EuropeTrip2026 and supplies #condo-bbq #maintenance_v2.");

        tags.Should().Equal("condo-bbq", "EuropeTrip2026", "maintenance_v2");
    }

    [Fact]
    public void ParseHashTags_IgnoresDuplicateTagsCaseInsensitively()
    {
        var tags = TransactionTagFormatter.ParseHashTags("#Maintenance #maintenance #MAINTENANCE");

        tags.Should().ContainSingle().Which.Should().Be("Maintenance");
    }

    [Theory]
    [InlineData("Dinner #", "")]
    [InlineData("Dinner #Eu", "Eu")]
    [InlineData("#maintenance", "maintenance")]
    [InlineData("Dinner without a tag", null)]
    public void GetActiveHashTagQuery_ReturnsOnlyTagAtEndOfText(string text, string? expected)
    {
        TransactionTagFormatter.GetActiveHashTagQuery(text).Should().Be(expected);
    }

    [Fact]
    public void ReplaceActiveHashTag_PreservesDescriptionAndEarlierTags()
    {
        var result = TransactionTagFormatter.ReplaceActiveHashTag(
            "Dinner #EuropeTrip2026 hotel #mai", "maintenance");

        result.Should().Be("Dinner #EuropeTrip2026 hotel #maintenance");
    }
}
