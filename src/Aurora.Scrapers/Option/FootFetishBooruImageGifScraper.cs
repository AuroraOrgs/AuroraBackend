using Aurora.Domain.Enums;
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
            scrapPage: document => Task.FromResult(FootfetishBooruBehaviour.FootFetishBooruScrap(document)),
            findMaxPageNumber: async (client) =>
            {
                var firstPage = await LoadPage(term, 0, client);
                return firstPage.PipeValue(document => FootfetishBooruBehaviour.ExtractFootfetishBooruPagesCount(document));
            });
    }

    private async Task<ValueOrNull<HtmlDocument>> LoadPage(string term, int pageNumber, HttpClient client)
    {
        var baseUrl = Website.GetBaseUrl();
        var pageUrl = $"{baseUrl}/index.php?page=post&s=list&tags={term}&pid={pageNumber * ScraperConstants.FootFetishBooruPostsPerPage}";
        return await client.TryLoadDocumentFromUrl(pageUrl);
    }

    private static string TermToUrlFormat(string term) =>
        term.Replace(" ", "_");
}
