using System.Text.Json;
using System.Text.RegularExpressions;

namespace Buenaventura.Shared;

public static class TransactionTagFormatter
{
    private static readonly Regex HashTagRegex = new(@"(?<![\w-])#(?<tag>[A-Za-z0-9][A-Za-z0-9_-]*)", RegexOptions.Compiled);
    private static readonly Regex ActiveHashTagRegex = new(@"(?:^|[\s,;])#(?<tag>[A-Za-z0-9_-]*)$", RegexOptions.Compiled);

    public static List<string> Normalize(IEnumerable<string>? tags)
    {
        return (tags ?? [])
            .Select(tag => tag.Trim())
            .Select(tag => tag.StartsWith('#') ? tag[1..] : tag)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static List<string> ParseTagText(string? tags)
    {
        return Normalize((tags ?? string.Empty)
            .Split([',', ';', ' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    public static List<string> ParseHashTags(string? text)
    {
        return Normalize(HashTagRegex
            .Matches(text ?? string.Empty)
            .Select(match => match.Groups["tag"].Value));
    }

    public static string? GetActiveHashTagQuery(string? text)
    {
        var match = ActiveHashTagRegex.Match(text ?? string.Empty);
        return match.Success ? match.Groups["tag"].Value : null;
    }

    public static string ReplaceActiveHashTag(string? text, string tag)
    {
        var value = text ?? string.Empty;
        var match = ActiveHashTagRegex.Match(value);
        if (!match.Success)
        {
            return value;
        }

        var normalizedTag = Normalize([tag]).SingleOrDefault();
        return normalizedTag == null
            ? value
            : $"{value[..match.Groups["tag"].Index]}{normalizedTag}";
    }

    public static string ToTagText(IEnumerable<string>? tags)
    {
        return string.Join(", ", Normalize(tags));
    }

    public static string Serialize(IEnumerable<string>? tags)
    {
        return JsonSerializer.Serialize(Normalize(tags));
    }

    public static List<string> Deserialize(string? serializedTags)
    {
        if (string.IsNullOrWhiteSpace(serializedTags))
        {
            return [];
        }

        try
        {
            return Normalize(JsonSerializer.Deserialize<List<string>>(serializedTags));
        }
        catch (JsonException)
        {
            return ParseTagText(serializedTags);
        }
    }

    public static bool Matches(string? serializedTags, IEnumerable<string>? includeTags, IEnumerable<string>? excludeTags)
    {
        var transactionTags = Deserialize(serializedTags);
        var includes = Normalize(includeTags);
        var excludes = Normalize(excludeTags);

        if (includes.Count > 0 && !includes.Any(include => transactionTags.Contains(include, StringComparer.OrdinalIgnoreCase)))
        {
            return false;
        }

        return excludes.Count == 0 || !excludes.Any(exclude => transactionTags.Contains(exclude, StringComparer.OrdinalIgnoreCase));
    }
}
