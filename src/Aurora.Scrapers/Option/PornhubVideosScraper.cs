using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Scrapers.Config;
using Aurora.Scrapers.Contracts;
using Aurora.Scrapers.Extensions;
using Microsoft.Extensions.Options;

namespace Aurora.Scrapers.Option
{
    public class PornhubVideosScraper : IOptionScraper
    {
        private readonly IHttpClientFactory _clientProvider;
        private readonly ScrapersConfig _config;

        public PornhubVideosScraper(IHttpClientFactory clientProvider, IOptions<ScrapersConfig> config)
        {
            _clientProvider = clientProvider;
            _config = config.Value;
        }

        public SupportedWebsite Website => SupportedWebsite.Pornhub;
        public IEnumerable<ContentType> ContentTypes { get; init; } = new List<ContentType>() { ContentType.Video };

        public async Task<List<SearchItem<SearchResultData>>> ScrapAsync(List<string> terms, CancellationToken token = default)
        {
            var baseUrl = Website.GetBaseUrl();

            List<SearchItem<SearchResultData>> videoItems = new();

            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var urlsCount = 0;

            using var client = _clientProvider.CreateClient(HttpClientNames.DefaultClient);
            //TODO: Implement scraping of all pages
            for (var i = 0; i < _config.MaxPagesCount; i++)
            {
                if (_config.UseLimitations && urlsCount >= _config.MaxItemsCount)
                {
                    break;
                }

                // e.g: https://www.pornhub.com/video/search?search=test+value&page=1
                var searchTerm = string.Join(" ", terms);
                var searchTermUrlFormatted = searchTerm.FormatTermToUrl();
                var searchPageUrl = $"{baseUrl}/video/search?search={searchTermUrlFormatted}&page={i + 1}";
                if (await client.TryLoadDocumentFromUrl(htmlDocument, searchPageUrl) == false)
                {
                    break;
                }

                var videoLinksNodes = htmlDocument.DocumentNode
                    ?.SelectNodes("//a[contains(@class, 'videoPreviewBg')]");

                if (videoLinksNodes is not null)
                {
                    foreach (var videoLinkNode in videoLinksNodes)
                    {
                        var currentLinkImageNode = videoLinkNode.ChildNodes
                            .FirstOrDefault(n => n.Name == "img");
                        //TODO: Add default image or make preview image nullable
                        string previewImage = null;

                        if (currentLinkImageNode is not null)
                        {
                            var currentLinkImageAttributes = currentLinkImageNode.Attributes;
                            previewImage = currentLinkImageAttributes["data-thumb_url"].Value;
                        }

                        var currentLinkAttributes = videoLinkNode.Attributes;
                        string itemUrl = $"{baseUrl}{currentLinkAttributes["href"].Value}";

                        urlsCount++;
                        videoItems.Add(new(ContentType.Video, previewImage, itemUrl));
                    }
                }

                await Task.Delay(250);
            }

            return videoItems;
        }
    }
}
