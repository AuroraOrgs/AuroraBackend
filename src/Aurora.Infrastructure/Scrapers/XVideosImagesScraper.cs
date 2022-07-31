using Aurora.Application.Contracts;
using Aurora.Application.Models;
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
    public class XVideosImagesScraper : IOptionScraper
    {
        private readonly IHttpClientFactory _clientProvider;
        private readonly IOptions<ScrapersConfig> _config;

        public XVideosImagesScraper(IHttpClientFactory clientProvider, IOptions<ScrapersConfig> config)
        {
            _clientProvider = clientProvider;
            _config = config;
        }

        public SupportedWebsite Website => SupportedWebsite.XVideos;
        public IEnumerable<SearchOption> Options { get; init; } = new List<SearchOption> { SearchOption.Image };

        public async Task<List<SearchItem>> ScrapAsync(string term, CancellationToken token = default)
        {
            var config = _config.Value;
            var baseUrl = Website.GetBaseUrl();
            List<SearchItem> imageItems = new();

            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var pageNumber = 1;

            var urlsCount = 0;

            using var client = _clientProvider.CreateClient(HttpClientNames.XVideosClient);
            for (var i = 0; i < config.MaxPagesCount; i++)
            {
                if (urlsCount >= config.MaxItemsCount)
                {
                    break;
                }

                // e.g: https://www.xvideos.com/?k=test+value&p=1
                var searchTermUrlFormatted = term.FormatTermToUrl();
                var searchPageUrl = $"{baseUrl}/?k={searchTermUrlFormatted}&p={pageNumber}";
                var htmlSearchPage = await client.TryLoadDocumentFromUrl(htmlDocument, searchPageUrl);

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
                        string imageUrl = currentLinkImageAttributes["data-src"]?.Value;
                        if (imageUrl is not null)
                        {
                            imageItems.Add(new(SearchOption.Image, imageUrl, imageUrl));
                        }
                    }

                    urlsCount++;
                }

                pageNumber++;
                await Task.Delay(250);
            }

            return imageItems;
        }
    }
}
