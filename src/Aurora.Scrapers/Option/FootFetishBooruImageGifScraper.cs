using Aurora.Scrapers.Behaviours;
using Aurora.Scrapers.Services;

namespace Aurora.Scrapers.Option;

public class FootFetishBooruImageGifScraper : IOptionScraper
{
    private readonly PagingRunner _runner;

    public FootFetishBooruImageGifScraper(PagingRunner runner)
    {
        _runner = runner;
    }

    public SupportedWebsite Website => SupportedWebsite.FootFetishBooru;
    public IEnumerable<ContentType> ContentTypes { get; init; } = new List<ContentType> { ContentType.Image, ContentType.Gif };

    public async Task<List<SearchItem>> ScrapAsync(List<string> terms, CancellationToken token = default)
    {
        string term = string.Join("+", terms.Select(TermToUrlFormat));
        return await _runner.RunPagingAsync(HttpClientNames.DefaultClient,
            loadPage: (pageNumber, client) => LoadPage(term, pageNumber, client),
            scrapPage: document => Task.FromResult(Scraping.FootFetishBooruScrap(document)),
            findMaxPageNumber: async (client) =>
            {
                var firstPage = await LoadPage(term, 0, client);
                return firstPage.PipeValue(document => ExtractPagesCount(document));
            });
    }

    private async Task<ValueOrNull<HtmlDocument>> LoadPage(string term, int pageNumber, HttpClient client)
    {
        var baseUrl = Website.GetBaseUrl();
        var pageUrl = $"{baseUrl}/index.php?page=post&s=list&tags={term}&pid={pageNumber * ScraperConstants.FootFetishBooruPostsPerPage}";
        return await client.TryLoadDocumentFromUrl(pageUrl);
    }

    internal static ValueOrNull<int> ExtractPagesCount(HtmlDocument searchPage)
    {
        ValueOrNull<int> result;
        var paginator = searchPage.DocumentNode.SelectSingleNode("//div[@id='paginator']");
        if (paginator is not null)
        {
            var lastButton = paginator.ChildNodes.Last();
            if (lastButton is not null && lastButton.GetAttributeValue("alt", "none") == "last page")
            {
                const string defVal = "none";
                var lastButtonReference = lastButton.GetAttributeValue("href", defVal);
                var pidPart = lastButtonReference.Split("&amp;").Where(x => x.StartsWith("pid")).FirstOrDefault();
                var lastPidStr = pidPart?.Split('=')?.LastOrDefault();
                if (lastPidStr is not null && int.TryParse(lastPidStr, out int lastPid))
                {
                    result = lastPid / ScraperConstants.FootFetishBooruPostsPerPage + 1;
                }
                else
                {
                    result = 1;
                }
            }
            else
            {
                result = ValueOrNull<int>.CreateNull();
            }
        }
        else
        {
            result = ValueOrNull<int>.CreateNull();
        }
        return result;
    }

    private static string TermToUrlFormat(string term) =>
        term.Replace(" ", "_");
}
