using Aurora.Domain.Enums;
using Aurora.Scrapers.Services;

namespace Aurora.Scrapers.Option;

public class PornhubImagesScraper : IOptionScraper
{
    private readonly DriverInitializer _initializer;
    private readonly PagingRunner _runner;

    public PornhubImagesScraper(DriverInitializer initializer, PagingRunner runner)
    {
        _initializer = initializer;
        _runner = runner;
    }

    public SupportedWebsite Website => SupportedWebsite.Pornhub;
    public IEnumerable<ContentType> ContentTypes { get; init; } = new List<ContentType> { ContentType.Image };

    public async Task<List<SearchItem>> ScrapAsync(List<string> terms, CancellationToken token = default)
    {
        var baseUrl = Website.GetBaseUrl();
        var driver = await _initializer.Initialize();
        var searchTerm = String.Join(" ", terms).FormatTermToUrl();
        //TODO: Implement scraping of all pages
        return await _runner.RunPagingAsync(HttpClientNames.DefaultClient,
            loadPage: async (pageNumber, client) => await TryLoadPage(baseUrl, searchTerm, pageNumber, client),
            scrapPage: document =>
            {
                List<SearchItem> items = new();
                var albumNodes = document.DocumentNode?.SelectNodes("//li[contains(@class,'photoAlbumListContainer')]/div/a");

                if (albumNodes is not null)
                {
                    const string noHref = "none";
                    var albums = albumNodes.Select(x => x.GetAttributeValue("href", noHref))
                                           .Where(x => x != noHref && x.Contains("album"));
                    foreach (var album in albums)
                    {
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
                        document.LoadHtml(albumHtml);

                        var images = document.DocumentNode
                            ?.SelectNodes("//ul[contains(@class, 'photosAlbumsListing')]/li/div");

                        if (images is not null)
                        {
                            foreach (var image in images.Where(x => x is not null))
                            {
                                var preview = image.GetAttributeValue("data-bkg");
                                var url = baseUrl + image.ChildNodes.Where(x => x.Name == "a").First().GetAttributeValue("href");
                                items.Add(new SearchItem(ContentType.Image, preview, url));
                            }
                        }
                    }
                }
                return Task.FromResult(items);
            });
    }

    private static async Task<ValueOrNull<HtmlDocument>> TryLoadPage(string baseUrl, string searchTerm, int pageNumber, HttpClient client)
    {
        var fullUrl = $"{baseUrl}/albums?search={searchTerm}&page={pageNumber + 1}";
        return await client.TryLoadDocumentFromUrl(fullUrl);
    }
}
