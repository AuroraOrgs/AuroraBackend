using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Extensions;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Scrapers
{
    public class XVideosImagesScraper : IOptionScraper
    {
        private readonly IWebClientService _clientProvider;
        private readonly IOptions<ScrapersConfig> _config;

        public XVideosImagesScraper(IWebClientService clientProvider, IOptions<ScrapersConfig> config)
        {
            _clientProvider = clientProvider;
            _config = config;
        }

        public SupportedWebsite Website => SupportedWebsite.Xvideos;
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

            using var client = await _clientProvider.Provide();
            for (var i = 0; i < config.MaxPagesCount; i++)
            {
                if (urlsCount >= config.MaxItemsCount)
                {
                    break;
                }

                // e.g: https://www.xvideos.com/?k=test+value&p=1
                var searchTermUrlFormatted = term.FormatTermToUrl();
                var searchPageUrl = $"{baseUrl}/?k={searchTermUrlFormatted}&p={pageNumber}";
                await _clientProvider.SetTls12UserString(client);
                var htmlSearchPage = client.DownloadString(searchPageUrl);
                htmlDocument.LoadHtml(htmlSearchPage);

                var bodyNode = htmlDocument.DocumentNode
                    ?.SelectSingleNode("//body");
                var videoLinksNodes = bodyNode
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
