using HtmlAgilityPack;
using System;
using System.Net;

namespace Aurora.Infrastructure.Extensions
{
    public static class WebClientExtensions
    {
        public static bool LoadDocumentFromUrl(this WebClient client, HtmlDocument htmlDocument, string searchPageUrl)
        {
            bool reachedEnd;
            try
            {
                var htmlSearchPage = client.DownloadString(searchPageUrl);
                htmlDocument.LoadHtml(htmlSearchPage);
                reachedEnd = false;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("404"))
                {
                    reachedEnd = true;
                }
                else
                {
                    throw;
                }
            }
            return reachedEnd;
        }
    }
}
