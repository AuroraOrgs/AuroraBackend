using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Extensions;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Scrapers
{
    public class XVideosVideosScraper : IOptionScraper
    {
        private readonly IHttpClientFactory _clientProvider;
        private readonly IOptions<ScrapersConfig> _config;

        public XVideosVideosScraper(IHttpClientFactory clientProvider, IOptions<ScrapersConfig> config)
        {
            _clientProvider = clientProvider;
            _config = config;
        }

        public SupportedWebsite Website => SupportedWebsite.XVideos;
        public IEnumerable<ContentType> ContentTypes { get; init; } = new List<ContentType>() { ContentType.Video };

        public async Task<List<SearchItem>> ScrapAsync(string term, CancellationToken token = default)
        {
            var baseUrl = Website.GetBaseUrl();
            var config = _config.Value;

            List<SearchItem> videoItems = new();

            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var pageNumber = 1;

            var urlsCount = 0;

            using var client = _clientProvider.CreateClient(HttpClientNames.XVideosClient);
            //TODO: Implement scraping of all pages
            for (var i = 0; i < config.MaxPagesCount; i++)
            {
                if (config.UseLimitations && urlsCount >= config.MaxItemsCount)
                {
                    break;
                }

                // e.g: https://www.xvideos.com/?k=test+value&p=1
                var searchTermUrlFormatted = term.FormatTermToUrl();
                var searchPageUrl = $"{baseUrl}/?k={searchTermUrlFormatted}&p={pageNumber}";
                if (await client.TryLoadDocumentFromUrl(htmlDocument, searchPageUrl) == false)
                {
                    break;
                }

                var videoLinksNodes = htmlDocument.DocumentNode
                    ?.SelectNodes("//a");

                if (videoLinksNodes is null)
                {
                    break;
                }

                foreach (var videoLinkNode in videoLinksNodes)
                {
                    var currentLinkImageNode = videoLinkNode.ChildNodes
                        .FirstOrDefault(n => n.Name == "img");

                    if (currentLinkImageNode is not null)
                    {
                        var currentLinkImageAttributes = currentLinkImageNode.Attributes;
                        string imagePreviewUrl = currentLinkImageAttributes["data-src"]?.Value;
                        if (imagePreviewUrl is not null)
                        {
                            var currentLinkAttributes = videoLinkNode.Attributes;
                            var videoLink = currentLinkAttributes["href"]?.Value;
                            if (videoLink is not null && videoLink.Contains("video") && !videoLink.Contains("videos"))
                            {
                                string searchItemUrl = $"{baseUrl}{videoLink}";
                                if (searchItemUrl is not null && imagePreviewUrl is not null)
                                {
                                    videoItems.Add(new(ContentType.Video, imagePreviewUrl, searchItemUrl));
                                }
                            }
                        }
                    }
                    urlsCount++;
                }

                pageNumber++;
                await Task.Delay(250);
            }

            return videoItems;
        }
    }
}
