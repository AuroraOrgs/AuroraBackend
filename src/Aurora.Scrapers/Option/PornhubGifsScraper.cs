using Aurora.Domain.Enums;
using Aurora.Scrapers.Services;

namespace Aurora.Scrapers.Option;

public class PornhubGifsScraper : IOptionScraper
{
    private const string _dateSourcePhncdn = "https://dl.phncdn.com";

    private readonly PagingRunner _runner;

    public PornhubGifsScraper(PagingRunner runner)
    {
        _runner = runner;
    }

    public SupportedWebsite Website => SupportedWebsite.Pornhub;
    public IEnumerable<ContentType> ContentTypes { get; init; } = new List<ContentType>() { ContentType.Gif };

    public async Task<List<SearchItem>> ScrapAsync(List<string> terms, CancellationToken token = default)
    {
        var baseUrl = Website.GetBaseUrl();
        return await _runner.RunPagingAsync(HttpClientNames.DefaultClient,
            loadPage: async (pageNumber, client) => await TryLoadPage(terms, baseUrl, client, pageNumber),
            scrapPage: document =>
            {
                var bodyNode = document.DocumentNode?.SelectSingleNode("//body");
                var gifLinksNodes = bodyNode?.SelectNodes("//li[contains(@class, 'gifVideoBlock')]");

                List<SearchItem> gifItems = new();
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
                    }
                }
                return Task.FromResult(gifItems);
            },
            findMaxPageNumber: async client =>
            {
                return (await TryLoadPage(terms, baseUrl, client, 0)).PipeValue(document =>
                {
                    ValueOrNull<int> result;
                    var itemsText = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'showingCounter')]").InnerText;
                    var numbers = itemsText.ExtractAllNumbers();
                    if (numbers.Count == 3)
                    {
                        var pageSize = numbers[1] - numbers[0] + 1;
                        var itemsCount = numbers[2];
                        var pagesCount = (int)Math.Ceiling((double)itemsCount / pageSize);
                        result = pagesCount;
                    }
                    else
                    {
                        result = $"Number of numbers for page count has unexpected value of '{numbers.Count}'".ToErrorResult<int>();
                    }
                    return result;
                });
            });
    }

    private static async Task<ValueOrNull<HtmlDocument>> TryLoadPage(List<string> terms, string baseUrl, HttpClient client, int i)
    {
        var term = string.Join(" ", terms);
        var searchTermUrlFormatted = term.FormatTermToUrl();
        var searchPageUrl = $"{baseUrl}/gifs/search?search={searchTermUrlFormatted}&page={i + 1}";
        return await client.TryLoadDocumentFromUrl(searchPageUrl);
    }
}
