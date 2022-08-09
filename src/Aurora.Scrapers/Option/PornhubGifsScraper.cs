﻿using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Scrapers.Config;
using Aurora.Scrapers.Contracts;
using Aurora.Scrapers.Extensions;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;

namespace Aurora.Scrapers.Option
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
        public IEnumerable<ContentType> ContentTypes { get; init; } = new List<ContentType>() { ContentType.Gif };

        public async Task<List<SearchItem>> ScrapAsync(List<string> terms, CancellationToken token = default)
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

            //TODO: Implement scraping of all pages
            for (var i = 0; i < config.MaxPagesCount; i++)
            {
                if (config.UseLimitations && urlsCount >= config.MaxItemsCount)
                {
                    break;
                }

                var currentPageNumber = i + 1;
                // e.g: https://www.pornhub.com/gifs/search?search=test+value&page=1
                var term = string.Join(" ", terms);
                var searchTermUrlFormatted = term.FormatTermToUrl();
                var searchPageUrl = $"{baseUrl}/gifs/search?search={searchTermUrlFormatted}&page={currentPageNumber}";
                if (await client.TryLoadDocumentFromUrl(htmlDocument, searchPageUrl) == false)
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
                            gifItems.Add(new(ContentType.Gif, imagePreviewUrl, searchItemUrl));
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