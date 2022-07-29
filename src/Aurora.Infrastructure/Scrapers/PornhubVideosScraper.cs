using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Scrapers
{
    public class PornhubVideosScraper : IOptionScraper
    {
        private readonly IWebClientService _clientProvider;
        private readonly ScrapersConfig _config;

        public PornhubVideosScraper(IWebClientService clientProvider, IOptions<ScrapersConfig> config)
        {
            _clientProvider = clientProvider;
            _config = config.Value;
        }

        public SupportedWebsite Website => SupportedWebsite.Pornhub;

        private List<SearchOption> _supportedOptions = new() { SearchOption.Video };
        public IEnumerable<SearchOption> Options => _supportedOptions;

        public async Task<List<SearchItem>> ScrapAsync(string searchTerm, CancellationToken token = default)
        {
            var baseUrl = Website.GetBaseUrl();

            List<SearchItem> videoItems = new();

            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var urlsCount = 0;

            using var client = await _clientProvider.Provide();
            for (var i = 0; i < _config.MaxPagesCount; i++)
            {
                if (urlsCount >= _config.MaxItemsCount)
                {
                    break;
                }

                // e.g: https://www.pornhub.com/video/search?search=test+value&page=1
                var searchTermUrlFormatted = searchTerm.FormatTermToUrl();
                var searchPageUrl = $"{baseUrl}/video/search?search={searchTermUrlFormatted}&page={i + 1}";
                await _clientProvider.SetDefaultUserString(client);
                bool isEnd = client.LoadDocumentFromUrl(htmlDocument, searchPageUrl);

                if (isEnd)
                {
                    break;
                }

                var bodyNode = htmlDocument.DocumentNode
                    ?.SelectSingleNode("//body");
                var videoLinksNodes = bodyNode
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
                        videoItems.Add(new(SearchOption.Video, previewImage, itemUrl));
                    }
                }

                await Task.Delay(250);
            }

            return videoItems;
        }
    }
}
