using HtmlAgilityPack;

namespace Aurora.Infrastructure.Extensions
{
    public static class HtmlNodeExtensions
    {
        public static string GetAttributeOrDefault(this HtmlNode node, string attributeName, string defValue = default) =>
            node.Attributes[attributeName]?.Value ?? defValue;
    }
}
