using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Extensions;
using Aurora.Infrastructure.Services;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Scrapers
{
    public class PornhubImagesScraper : IOptionScraper
    {
        private readonly DriverInitializer _initializer;
        private readonly IOptions<ScrapersConfig> _config;
        private readonly IWebClientService _clientProvider;

        public PornhubImagesScraper(DriverInitializer initializer, IOptions<ScrapersConfig> config, IWebClientService clientProvider)
        {
            _initializer = initializer;
            _config = config;
            _clientProvider = clientProvider;
        }

        public SupportedWebsite Website => SupportedWebsite.Pornhub;

        private IEnumerable<SearchOption> _options = new List<SearchOption> { SearchOption.Image };

        public IEnumerable<SearchOption> Options => _options;

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

            using var client = await _clientProvider.Provide();
            for (var i = 0; i < config.MaxPagesCount; i++)
            {
                var pageNumber = i + 1;
                var fullUrl = $"{baseUrl}/albums?search={searchTerm.FormatTermToUrl()}&page={pageNumber}";
                await _clientProvider.SetDefaultUserString(client);
                var html = client.DownloadString(fullUrl);
                htmlDocument.LoadHtml(html);

                var albumNodes = htmlDocument.DocumentNode
                    ?.SelectNodes("//li[contains(@class,'photoAlbumListContainer')]/div/a");

                if (albumNodes is null) continue;
                if (result.Count >= config.MaxItemsCount) break;

                const string noHref = "none";
                var albums = albumNodes.Select(x => x.GetAttributeOrDefault("href", noHref))
                                       .Where(x => x != noHref && x.Contains("album"));
                foreach (var album in albums)
                {
                    if (result.Count >= config.MaxItemsCount) break;

                    await _clientProvider.SetDefaultUserString(client);
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
