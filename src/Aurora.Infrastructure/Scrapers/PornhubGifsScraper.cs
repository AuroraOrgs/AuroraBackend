using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Extensions;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Scrapers
{
    public class PornhubGifsScraper : IOptionScraper
    {
        private const string _dateSourcePhncdn = "https://dl.phncdn.com";

        private readonly IHttpClientFactory _clientProvider;
        private readonly IOptions<ScrapersConfig> _config;

        public PornhubGifsScraper(IHttpClientFactory clientProvider, IOptions<ScrapersConfig> config)
        {
            _clientProvider = clientProvider;
            _config = config;
        }

        public SupportedWebsite Website => SupportedWebsite.Pornhub;
        public IEnumerable<SearchOption> Options { get; init; } = new List<SearchOption>() { SearchOption.Gif };

        public async Task<List<SearchItem>> ScrapAsync(string term, CancellationToken token = default)
        {
            var config = _config.Value;
            var baseUrl = Website.GetBaseUrl();
            List<SearchItem> gifItems = new();

            var htmlDocument = new HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var urlsCount = 0;

            using var client = _clientProvider.CreateClient(HttpClientNames.DefaultClient);

            for (var i = 0; i < config.MaxPagesCount; i++)
            {
                if (urlsCount >= config.MaxItemsCount)
                {
                    break;
                }

                var currentPageNumber = i + 1;
                // e.g: https://www.pornhub.com/gifs/search?search=test+value&page=1
                var searchTermUrlFormatted = term.FormatTermToUrl();
                var searchPageUrl = $"{baseUrl}/gifs/search?search={searchTermUrlFormatted}&page={currentPageNumber}";
                bool isEnd = await client.TryLoadDocumentFromUrl(htmlDocument, searchPageUrl);
                if (isEnd)
                {
                    break;
                }
                var bodyNode = htmlDocument.DocumentNode
                    ?.SelectSingleNode("//body");
                var gifLinksNodes = bodyNode
                    ?.SelectNodes("//li[contains(@class, 'gifVideoBlock')]");

                if (gifLinksNodes is not null)
                {
                    foreach (var gifLinkNode in gifLinksNodes)
                    {
                        var currentLinkGifNode = gifLinkNode.ChildNodes
                            .FirstOrDefault(n => n.Name == "a");

                        if (currentLinkGifNode is not null)
                        {
                            var currentLinkImageAttributes = currentLinkGifNode.Attributes;
                            string imagePreviewUrl = $"{baseUrl}{currentLinkImageAttributes["href"].Value}";
                            string searchItemUrl = $"{_dateSourcePhncdn}{currentLinkImageAttributes["href"].Value}.gif";
                            gifItems.Add(new(SearchOption.Gif, imagePreviewUrl, searchItemUrl));
                        }

                        urlsCount++;
                    }
                }

                await Task.Delay(250);
            }

            return gifItems;
        }
    }
}
