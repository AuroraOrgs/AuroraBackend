using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Extensions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Scrapers
{
    public class FootFetishBooruImageGifScraper : IOptionScraper
    {
        private const int _itemsPerPage = 20;

        private static List<SearchItem> _emptyResult = new List<SearchItem>();

        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<FootFetishBooruImageGifScraper> _logger;
        private readonly IOptions<ScrapersConfig> _config;

        public FootFetishBooruImageGifScraper(IHttpClientFactory clientFactory, ILogger<FootFetishBooruImageGifScraper> logger, IOptions<ScrapersConfig> config)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _config = config;
        }

        public SupportedWebsite Website => SupportedWebsite.FootFetishBooru;
        public IEnumerable<SearchOption> Options { get; init; } = new List<SearchOption> { SearchOption.Image, SearchOption.Gif };

        public async Task<List<SearchItem>> ScrapAsync(string term, CancellationToken token = default)
        {
            var config = _config.Value;
            using var client = _clientFactory.CreateClient(HttpClientNames.DefaultClient);
            var baseUrl = Website.GetBaseUrl();
            var fullUrl = $"{baseUrl}/index.php?page=post&s=list&tags={TermToUrlFormat(term)}";
            var htmlDocument = new HtmlDocument
            {
                OptionFixNestedTags = true
            };

            List<SearchItem> result;
            if (await client.TryLoadDocumentFromUrl(htmlDocument, fullUrl))
            {
                var paginator = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='paginator']");
                if (paginator is not null)
                {
                    var lastButton = paginator.ChildNodes.Last();
                    //multiple pages
                    if (lastButton is not null && lastButton.GetAttributeValue("alt", "none") == "last page")
                    {
                        const string defVal = "none";
                        var lastButtonReference = lastButton.GetAttributeValue("href", defVal);
                        var pidPart = lastButtonReference.Split("&amp;").Where(x => x.StartsWith("pid")).FirstOrDefault();
                        var lastPidStr = pidPart?.Split('=')?.LastOrDefault();
                        if (lastPidStr is not null && Int32.TryParse(lastPidStr, out int lastPid))
                        {
                            var pagesCount = lastPid / _itemsPerPage + 1;
                            var lastPageIndex = Math.Min(pagesCount, config.MaxPagesCount);
                            var items = new List<SearchItem>();
                            for (int i = 0; i < lastPageIndex; i++)
                            {
                                items.AddRange(await LoadPageAsync(term, client, i));
                                if (items.Count > config.MaxItemsCount)
                                {
                                    break;
                                }
                            }
                            result = items;
                        }
                        else
                        {
                            _logger.LogInformation("Failed to parse index of the last image for term '{term}' at '{url}'", term, fullUrl);
                            result = await LoadPageAsync(term, client, 0);
                        }
                    }
                    //single page
                    else
                    {
                        result = await LoadPageAsync(term, client, 0);
                    }
                }
                else
                {
                    result = _emptyResult;
                }
            }
            else
            {
                result = _emptyResult;
            }
            return result;
        }

        private async Task<List<SearchItem>> LoadPageAsync(string term, HttpClient client, int pageNumber)
        {
            var htmlDocument = new HtmlDocument
            {
                OptionFixNestedTags = true
            };
            var baseUrl = Website.GetBaseUrl();
            var items = new List<SearchItem>();
            var pageUrl = $"{baseUrl}/index.php?page=post&s=list&tags={TermToUrlFormat(term)}&pid={pageNumber * _itemsPerPage}";
            if (await client.TryLoadDocumentFromUrl(htmlDocument, pageUrl))
            {
                var posts = htmlDocument.DocumentNode.SelectNodes("//a[@id]")
                    .Where(x => x.Id.StartsWith("p") && x.Id != "pi");
                foreach (var post in posts)
                {
                    var hrefValue = post.GetAttributeValue("href", "none");
                    var location = $"{baseUrl}/{hrefValue}";
                    var previewImage = post.ChildNodes.Where(x => x.Name == "img").FirstOrDefault();
                    var previewSrc = previewImage.GetAttributeValue("src", "none");
                    SearchOption option;
                    if (previewSrc.EndsWith("gif"))
                    {
                        option = SearchOption.Gif;
                    }
                    else
                    {
                        option = SearchOption.Image;
                    }
                    var item = new SearchItem(option, previewSrc, location);
                    items.Add(item);
                }
            }
            return items;
        }

        private static string TermToUrlFormat(string term) =>
            term.Replace(" ", "_");
    }
}
