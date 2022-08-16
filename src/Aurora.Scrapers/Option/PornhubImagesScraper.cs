using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Scrapers.Config;
using Aurora.Scrapers.Contracts;
using Aurora.Scrapers.Extensions;
using Aurora.Scrapers.Services;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;

namespace Aurora.Scrapers.Option;

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
    public IEnumerable<ContentType> ContentTypes { get; init; } = new List<ContentType> { ContentType.Image };

    public async Task<List<SearchItem<SearchResultData>>> ScrapAsync(List<string> terms, CancellationToken token = default)
    {
        var config = _config.Value;
        var baseUrl = Website.GetBaseUrl();

        var htmlDocument = new HtmlDocument
        {
            OptionFixNestedTags = true
        };

        var driver = await _initializer.Initialize();
        var term = string.Join(" ", terms);
        var searchTerm = term.FormatTermToUrl();
        var result = new List<SearchItem<SearchResultData>>();

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
            var albums = albumNodes.Select(x => x.GetAttributeValue("href", noHref))
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
                        var preview = image.GetAttributeValue("data-bkg");
                        var url = baseUrl + image.ChildNodes.Where(x => x.Name == "a").First().GetAttributeValue("href");
                        result.Add(new SearchItem<SearchResultData>(ContentType.Image, preview, url));
                    }
                }
            }
        }

        return result;
    }
}
