using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Aurora.Infrastructure.Scrapers
{
    public class PornhubScraper : ScraperBase
    {
        private const string _baseUrl = "https://www.pornhub.com";

        public PornhubScraper(ILogger<ISearchScraper> logger) : base(logger)
        {
        }

        public override async Task<ValueOrNull<SearchResult>> SearchInner(SearchRequest request,
            CancellationToken token = default)
        {
            SearchResult result = new()
            {
                Website = _baseUrl
            };

            try
            {
                if (request.SearchOptions.Contains(SearchOption.Video))
                    result.Items.AddRange(ScrapVideos(request.SearchTerm, request.ResponseWebsitesMaxCount));
                if (request.SearchOptions.Contains(SearchOption.Gif))
                    result.Items.AddRange(ScrapGifs(request.SearchTerm, request.ResponseWebsitesMaxCount));
                if (request.SearchOptions.Contains(SearchOption.Image))
                    result.Items.AddRange(ScrapImages(request.SearchTerm, request.ResponseWebsitesMaxCount));
            }
            catch (Exception exception)
            {
                return ValueOrNull<SearchResult>.CreateNull(exception.Message);
            }

            result.CountItems = result.Items.Count;
            return ValueOrNull<SearchResult>.CreateValue(result);
        }
        
        private static IEnumerable<SearchItem> ScrapVideos(string searchTerm, int maxNumberOfVideoUrls)
        {
            List<SearchItem> videoItems = new();

            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var page = 1;
            const int pageNumberLimit = 5;

            using WebClient client = new();

            while (true)
            {
                // e.g: https://www.pornhub.com/video/search?search=test+value&page=1
                var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                var searchPageUrl = $"{_baseUrl}/video/search?search={searchTermUrlFormatted}&page={page}";

                ConfigureSecurityAccessToWebsite(client);
                var htmlSearchPage = client.DownloadString(searchPageUrl);
                htmlDocument.LoadHtml(htmlSearchPage);

                var bodyNode = htmlDocument.DocumentNode?.SelectSingleNode("//body");
                var videoLinksNodes = bodyNode?.SelectNodes("//a[contains(@class, 'videoPreviewBg')]");

                if (videoLinksNodes is null || page == pageNumberLimit)
                    break;

                for (var i = 0; i < maxNumberOfVideoUrls - videoItems.Count; i++)
                {
                    if (videoLinksNodes.Count == i + 1)
                        break;

                    var currentElement = videoLinksNodes.ElementAt(i);
                    if (currentElement is null || currentElement.Name != "a")
                        continue;

                    SearchItem searchVideoItem = new()
                    {
                        Option = SearchOption.Video
                    };

                    var currentLinkImageNode = currentElement.ChildNodes
                        .FirstOrDefault(n => n.Name == "img");

                    if (currentLinkImageNode != null)
                    {
                        var currentLinkImageAttributes = currentLinkImageNode.Attributes;
                        searchVideoItem.ImagePreviewUrl = currentLinkImageAttributes["data-thumb_url"].Value;
                    }

                    var currentLinkAttributes = currentElement.Attributes;
                    searchVideoItem.SearchItemUrl = $"{_baseUrl}{currentLinkAttributes["href"].Value}";

                    videoItems.Add(searchVideoItem);
                }

                page++;
                Thread.Sleep(1000);
            }

            return videoItems;
        }

        private static IEnumerable<SearchItem> ScrapImages(string requestSearchTerm, int requestResponseWebsitesMaxCount)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<SearchItem> ScrapGifs(string requestSearchTerm, int requestResponseWebsitesMaxCount)
        {
            throw new NotImplementedException();
        }
        
        private static void ConfigureSecurityAccessToWebsite(WebClient client)
        {
            client.Headers.Add("user-agent",
                "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
        }

        private static string FormatTermToUrl(string term)
        {
            return term.Replace(" ", "+");
        }
    }
}