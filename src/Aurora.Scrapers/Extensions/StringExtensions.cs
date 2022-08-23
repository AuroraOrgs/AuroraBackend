using System.Text.RegularExpressions;

namespace Aurora.Scrapers.Extensions;

internal static class StringExtensions
{
    public static string FormatTermToUrl(this string term) => term.Replace(" ", "+");

    private static Regex _numbersRegex = new Regex("\\d+(\\.\\d+)?", RegexOptions.Multiline | RegexOptions.Compiled);
    public static List<long> ExtractAllNumbers(this string str)
    {
        List<long> results = new();
        var matches = _numbersRegex.Matches(str);
        foreach (Match match in matches)
        {
            if (Int64.TryParse(match.Value, out long value))
            {
                results.Add(value);
            }
        }
        return results;
    }
}
