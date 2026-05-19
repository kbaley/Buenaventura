using System.Globalization;
using System.Text.RegularExpressions;

namespace Buenaventura.Shared;

public static class PastedTransactionParser
{
    private enum DateOrder
    {
        MonthDayYear,
        DayMonthYear
    }

    private sealed record ParsedLine(string RawDate, string Description, decimal Amount);
    private sealed record DateParts(int First, int Second, int Year);

    private static readonly Regex PastedTransactionPattern = new(
        @"^(?<date>\d{1,2}/\d{1,2}/\d{4})\t(?<description>.+?)\t(?<amount>-?[\d,]+(?:\.\d{2})?)\s+USD$",
        RegexOptions.Compiled);

    public static List<PendingTransactionModel> Parse(string pastedText)
    {
        return Parse(pastedText, DateTime.Today);
    }

    public static List<PendingTransactionModel> Parse(string pastedText, DateTime currentDate)
    {
        var parsedLines = ParseLines(pastedText);
        var dateOrder = InferDateOrder(parsedLines.Select(l => l.RawDate), currentDate);
        var transactions = new List<PendingTransactionModel>();

        foreach (var parsedLine in parsedLines)
        {
            if (!TryParseDate(parsedLine.RawDate, dateOrder, out var transactionDate))
            {
                continue;
            }

            transactions.Add(new PendingTransactionModel
            {
                TransactionDate = transactionDate,
                Description = parsedLine.Description,
                Amount = parsedLine.Amount,
                IsSelected = true
            });
        }

        return transactions;
    }

    private static List<ParsedLine> ParseLines(string pastedText)
    {
        var parsedLines = new List<ParsedLine>();
        var lines = pastedText
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            var match = PastedTransactionPattern.Match(line);
            if (!match.Success)
            {
                continue;
            }

            if (!decimal.TryParse(match.Groups["amount"].Value.Replace(",", ""), NumberStyles.Number | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out var amount))
            {
                continue;
            }

            parsedLines.Add(new ParsedLine(
                match.Groups["date"].Value,
                match.Groups["description"].Value.Trim(),
                amount));
        }

        return parsedLines;
    }

    private static DateOrder InferDateOrder(IEnumerable<string> rawDates, DateTime currentDate)
    {
        var dateParts = rawDates
            .Select(TryGetDateParts)
            .Where(p => p is not null)
            .Cast<DateParts>()
            .ToList();

        if (dateParts.Count == 0)
        {
            return DateOrder.MonthDayYear;
        }

        var monthDayScore = 0;
        var dayMonthScore = 0;
        var previousMonth = currentDate.AddMonths(-1).Month;

        foreach (var part in dateParts)
        {
            var monthDayValid = TryCreateDate(part.Year, part.First, part.Second, out var monthDayDate);
            var dayMonthValid = TryCreateDate(part.Year, part.Second, part.First, out var dayMonthDate);

            if (monthDayValid && !dayMonthValid)
            {
                monthDayScore += 20;
                continue;
            }

            if (dayMonthValid && !monthDayValid)
            {
                dayMonthScore += 20;
                continue;
            }

            if (!monthDayValid || !dayMonthValid)
            {
                continue;
            }

            monthDayScore += MonthClueScore(part.First, currentDate.Month, previousMonth);
            dayMonthScore += MonthClueScore(part.Second, currentDate.Month, previousMonth);
            monthDayScore += RecencyScore(monthDayDate, currentDate);
            dayMonthScore += RecencyScore(dayMonthDate, currentDate);
        }

        AddRepeatedComponentScores(dateParts, currentDate, previousMonth, ref monthDayScore, ref dayMonthScore);

        return dayMonthScore > monthDayScore ? DateOrder.DayMonthYear : DateOrder.MonthDayYear;
    }

    private static void AddRepeatedComponentScores(
        IReadOnlyCollection<DateParts> dateParts,
        DateTime currentDate,
        int previousMonth,
        ref int monthDayScore,
        ref int dayMonthScore)
    {
        var firstMode = GetMode(dateParts.Select(p => p.First));
        var secondMode = GetMode(dateParts.Select(p => p.Second));
        var majorityThreshold = dateParts.Count / 2;

        if (firstMode.Count > majorityThreshold)
        {
            monthDayScore += firstMode.Count * 4;
            monthDayScore += MonthClueScore(firstMode.Value, currentDate.Month, previousMonth) * firstMode.Count;
        }

        if (secondMode.Count > majorityThreshold)
        {
            dayMonthScore += secondMode.Count * 4;
            dayMonthScore += MonthClueScore(secondMode.Value, currentDate.Month, previousMonth) * secondMode.Count;
        }
    }

    private static (int Value, int Count) GetMode(IEnumerable<int> values)
    {
        return values
            .GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => (Value: g.Key, Count: g.Count()))
            .FirstOrDefault();
    }

    private static int MonthClueScore(int value, int currentMonth, int previousMonth)
    {
        if (value == currentMonth)
        {
            return 5;
        }

        return value == previousMonth ? 3 : 0;
    }

    private static int RecencyScore(DateTime parsedDate, DateTime currentDate)
    {
        var daysFromCurrentDate = (currentDate.Date - parsedDate.Date).TotalDays;
        return daysFromCurrentDate switch
        {
            >= 0 and <= 45 => 4,
            > 45 and <= 120 => 2,
            > 120 and <= 370 => 1,
            < 0 and >= -1 => 1,
            < -1 => -4,
            _ => 0
        };
    }

    private static DateParts? TryGetDateParts(string rawDate)
    {
        var parts = rawDate.Split('/');
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out var first) ||
            !int.TryParse(parts[1], out var second) ||
            !int.TryParse(parts[2], out var year))
        {
            return null;
        }

        return new DateParts(first, second, year);
    }

    private static bool TryParseDate(string rawDate, DateOrder dateOrder, out DateTime date)
    {
        date = default;
        var parts = TryGetDateParts(rawDate);
        return parts is not null && (dateOrder == DateOrder.MonthDayYear
            ? TryCreateDate(parts.Year, parts.First, parts.Second, out date)
            : TryCreateDate(parts.Year, parts.Second, parts.First, out date));
    }

    private static bool TryCreateDate(int year, int month, int day, out DateTime date)
    {
        date = default;
        if (year is < 1 or > 9999 ||
            month is < 1 or > 12 ||
            day < 1 ||
            day > DateTime.DaysInMonth(year, month))
        {
            return false;
        }

        date = new DateTime(year, month, day);
        return true;
    }
}
