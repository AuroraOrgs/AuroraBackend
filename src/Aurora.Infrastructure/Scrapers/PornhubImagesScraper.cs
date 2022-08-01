using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Extensions;
using Aurora.Infrastructure.Services;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Scrapers
{
    public class PornhubImagesScraper : IOptionScraper
    {
        private readonly DriverInitializer _initializer;
        private readonly IOptions<ScrapersConfig> _config;
        private readonly IHttpClientFactory _clientProvider;

        public PornhubImagesScraper(DriverInitializer initializer, IOptions<ScrapersConfig> config, IHttpClientFactory clientProvider)
        {
            _initializer = initializer;
            _config = config;
            _clientProvider = clientProvider;
        }

        public SupportedWebsite Website => SupportedWebsite.Pornhub;
        public IEnumerable<SearchOption> Options { get; init; } = new List<SearchOption> { SearchOption.Image };

        public async Task<List<SearchItem>> ScrapAsync(string term, CancellationToken token = default)
        {
            var config = _config.Value;
            var baseUrl = Website.GetBaseUrl();

            var htmlDocument = new HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var driver = await _initializer.Initialize();
            var searchTerm = term.FormatTermToUrl();
            var result = new List<SearchItem>();

            using var client = _clientProvider.CreateClient(HttpClientNames.DefaultClient);
            //TODO: Implement scraping of all pages
            for (var i = 0; i < config.MaxPagesCount; i++)
            {
                var pageNumber = i + 1;
                var fullUrl = $"{baseUrl}/albums?search={searchTerm.FormatTermToUrl()}&page={pageNumber}";
                if (await client.TryLoadDocumentFromUrl(htmlDocument, fullUrl) == false)
                {
                    break;
                }

                var albumNodes = htmlDocument.DocumentNode
                    ?.SelectNodes("//li[contains(@class,'photoAlbumListContainer')]/div/a");

                if (albumNodes is null) continue;

                const string noHref = "none";
                var albums = albumNodes.Select(x => x.GetAttributeOrDefault("href", noHref))
                                       .Where(x => x != noHref && x.Contains("album"));
                foreach (var album in albums)
                {
                    if (config.UseLimitations && result.Count >= config.MaxItemsCount) break;

                    var albumUrl = $"{baseUrl}{album}";
                    try
                    {
                        driver.Navigate().GoToUrl(albumUrl);
                    }
                    catch
                    {
                        //most likely non-album got found
                        continue;
                    }

                    var albumHtml = driver.PageSource;
                    htmlDocument.LoadHtml(albumHtml);

                    var images = htmlDocument.DocumentNode
                        ?.SelectNodes("//ul[contains(@class, 'photosAlbumsListing')]/li/div");

                    if (images is not null)
                    {
                        foreach (var image in images.Where(x => x is not null))
                        {
                            var preview = image.GetAttributeOrDefault("data-bkg");
                            var url = baseUrl + image.ChildNodes.Where(x => x.Name == "a").FirstOrDefault().GetAttributeOrDefault("href");
                            result.Add(new SearchItem(SearchOption.Image, preview, url));
                        }
                    }
                }
            }

            return result;
        }
    }
}
