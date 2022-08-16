namespace Aurora.Scrapers.Option;

public class XVideosImagesScraper : IOptionScraper
{
    private readonly IHttpClientFactory _clientProvider;
    private readonly IOptions<ScrapersConfig> _config;

    public XVideosImagesScraper(IHttpClientFactory clientProvider, IOptions<ScrapersConfig> config)
    {
        _clientProvider = clientProvider;
        _config = config;
    }

    public SupportedWebsite Website => SupportedWebsite.XVideos;
    public IEnumerable<ContentType> ContentTypes { get; init; } = new List<ContentType> { ContentType.Image };

    public async Task<List<SearchItem<SearchResultData>>> ScrapAsync(List<string> terms, CancellationToken token = default)
    {
        var config = _config.Value;
        var baseUrl = Website.GetBaseUrl();
        List<SearchItem<SearchResultData>> imageItems = new();

        var htmlDocument = new HtmlAgilityPack.HtmlDocument
        {
            OptionFixNestedTags = true
        };

        var pageNumber = 1;

        var urlsCount = 0;

        using var client = _clientProvider.CreateClient(HttpClientNames.XVideosClient);
        //TODO: Implement scraping of all pages
        for (var i = 0; i < config.MaxPagesCount; i++)
        {
            if (config.UseLimitations && urlsCount >= config.MaxItemsCount)
            {
                break;
            }

            // e.g: https://www.xvideos.com/?k=test+value&p=1
            var term = string.Join(" ", terms);
            var searchTermUrlFormatted = term.FormatTermToUrl();
            var searchPageUrl = $"{baseUrl}/?k={searchTermUrlFormatted}&p={pageNumber}";
            if (await client.TryLoadDocumentFromUrl(htmlDocument, searchPageUrl) == false)
            {
                break;
            }

            var videoLinksNodes = htmlDocument.DocumentNode
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
                    string imageUrl = currentLinkImageAttributes["data-src"]?.Value ?? "";
                    if (imageUrl is not null)
                    {
                        imageItems.Add(new(ContentType.Image, imageUrl, imageUrl));
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
