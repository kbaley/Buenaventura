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
}
