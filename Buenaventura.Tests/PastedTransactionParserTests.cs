using Buenaventura.Shared;
using FluentAssertions;
using Xunit;

namespace Buenaventura.Tests;

public class PastedTransactionParserTests
{
    [Fact]
    public void Parse_WhenSecondDateComponentRepeats_InfersDayMonthYear()
    {
        var pastedText = string.Join('\n',
            "03/05/2026\tNETFLIX.COM 866-579-717\t13.99 USD",
            "06/05/2026\tUBER TRIP\t7.50 USD",
            "08/05/2026\tAMAZON MARKETPLACE\t42.10 USD",
            "11/05/2026\tSPOTIFY\t10.99 USD");

        var transactions = PastedTransactionParser.Parse(pastedText, new DateTime(2026, 5, 19));

        transactions.Select(t => t.TransactionDate).Should().Equal(
            new DateTime(2026, 5, 3),
            new DateTime(2026, 5, 6),
            new DateTime(2026, 5, 8),
            new DateTime(2026, 5, 11));
    }

    [Fact]
    public void Parse_WhenFirstDateComponentRepeats_InfersMonthDayYear()
    {
        var pastedText = string.Join('\n',
            "05/03/2026\tNETFLIX.COM 866-579-717\t13.99 USD",
            "05/06/2026\tUBER TRIP\t7.50 USD",
            "05/08/2026\tAMAZON MARKETPLACE\t42.10 USD",
            "05/11/2026\tSPOTIFY\t10.99 USD");

        var transactions = PastedTransactionParser.Parse(pastedText, new DateTime(2026, 5, 19));

        transactions.Select(t => t.TransactionDate).Should().Equal(
            new DateTime(2026, 5, 3),
            new DateTime(2026, 5, 6),
            new DateTime(2026, 5, 8),
            new DateTime(2026, 5, 11));
    }

    [Fact]
    public void Parse_WhenDateIsUnambiguous_UsesOnlyValidOrder()
    {
        var pastedText = "13/05/2026\tGROCERY STORE\t64.25 USD";

        var transactions = PastedTransactionParser.Parse(pastedText, new DateTime(2026, 5, 19));

        transactions.Single().TransactionDate.Should().Be(new DateTime(2026, 5, 13));
    }

    [Fact]
    public void Parse_UsesCurrentDateToPreferRecentTransactions()
    {
        var pastedText = string.Join('\n',
            "03/05/2026\tNETFLIX.COM 866-579-717\t13.99 USD",
            "04/05/2026\tUBER TRIP\t7.50 USD");

        var transactions = PastedTransactionParser.Parse(pastedText, new DateTime(2026, 5, 19));

        transactions.Select(t => t.TransactionDate).Should().Equal(
            new DateTime(2026, 5, 3),
            new DateTime(2026, 5, 4));
    }
}
