namespace Aurora.Scrapers.Option;

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

    public async Task<List<SearchItem<SearchResultData>>> ScrapAsync(List<string> terms, CancellationToken token = default)
    {
        var config = _config.Value;
        var baseUrl = Website.GetBaseUrl();
        List<SearchItem<SearchResultData>> gifItems = new();

        var htmlDocument = new HtmlDocument
        {
            OptionFixNestedTags = true
        };

        var urlsCount = 0;

        using var client = _clientProvider.CreateClient(HttpClientNames.DefaultClient);

        int maxPageNumber = await FindMaxPageNumber(terms, config, baseUrl, htmlDocument, client);
        for (var i = 0; i < maxPageNumber; i++)
        {
            if (config.UseLimitations && urlsCount >= config.MaxItemsCount)
            {
                break;
            }

            // e.g: https://www.pornhub.com/gifs/search?search=test+value&page=1
            if (await TryLoadPage(terms, baseUrl, htmlDocument, client, i) == false)
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

    private static async Task<int> FindMaxPageNumber(List<string> terms, ScrapersConfig config, string baseUrl, HtmlDocument htmlDocument, HttpClient client)
    {
        int maxPageNumber;

        if (config.UseLimitations == false)
        {
            if (await TryLoadPage(terms, baseUrl, htmlDocument, client, 0))
            {
                var itemsText = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'showingCounter')]").InnerText;
                var numbers = itemsText.ExtractAllNumbers();
                if (numbers.Count == 3)
                {
                    var pageSize = numbers[1] - numbers[0] + 1;
                    var itemsCount = numbers[2];
                    var pagesCount = (int)Math.Ceiling((double)itemsCount / pageSize);
                    maxPageNumber = pagesCount;
                }
                else
                {
                    maxPageNumber = config.MaxItemsCount;
                }
            }
            else
            {
                maxPageNumber = config.MaxItemsCount;
            }
        }
        else
        {
            maxPageNumber = config.MaxItemsCount;
        }

        return maxPageNumber;
    }

    private static async Task<bool> TryLoadPage(List<string> terms, string baseUrl, HtmlDocument htmlDocument, HttpClient client, int i)
    {
        var term = string.Join(" ", terms);
        var searchTermUrlFormatted = term.FormatTermToUrl();
        var searchPageUrl = $"{baseUrl}/gifs/search?search={searchTermUrlFormatted}&page={i + 1}";
        return await client.TryLoadDocumentFromUrl(htmlDocument, searchPageUrl);
    }
}
