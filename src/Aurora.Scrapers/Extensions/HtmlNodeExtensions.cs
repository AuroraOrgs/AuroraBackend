﻿using HtmlAgilityPack;

namespace Aurora.Scrapers.Extensions
{
    public static class HtmlNodeExtensions
    {
        public static string GetAttributeValue(this HtmlNode node, string attributeName, string defValue = default) =>
            node.Attributes[attributeName]?.Value ?? defValue;
    }
}