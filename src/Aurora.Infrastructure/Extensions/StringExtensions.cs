namespace Aurora.Infrastructure.Extensions
{
    internal static class StringExtensions
    {
        public static string FormatTermToUrl(this string term) => term.Replace(" ", "+");
    }
}
