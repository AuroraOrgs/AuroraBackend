using Aurora.Scrapers.Behaviours;
using Aurora.Scrapers.Services;

namespace Aurora.Scrapers.Total;

public class FootFetishBooruTotalScraper : ITotalScraper
{
    private readonly PagingRunner _runner;

    public FootFetishBooruTotalScraper(PagingRunner runner)
    {
        _runner = runner;
    }

    public SupportedWebsite Website => SupportedWebsite.FootFetishBooru;

    public async Task<IEnumerable<(List<string> Terms, List<SearchItem> Items)>> Scrap()
    {
        var baseUrl = Website.GetBaseUrl();
        var fullUrl = $"{baseUrl}/index.php?page=post&s=list";
        var items = await _runner.RunPagingAsync(HttpClientNames.DefaultClient,
            loadPage: async (pageNumber, client) =>
            {
                var pageUrl = $"{fullUrl}&pid={pageNumber * ScraperConstants.FootFetishBooruPostsPerPage}";
                return await client.TryLoadDocumentFromUrl(pageUrl);
            },
            scrapPage: document => Task.FromResult(FootfetishBooruBehaviour.FootFetishBooruScrap(document)),
            findMaxPageNumber: async client =>
            {
                var document = await client.TryLoadDocumentFromUrl(fullUrl);
                return document.PipeValue(value => FootfetishBooruBehaviour.ExtractFootfetishBooruPagesCount(value));
            });

        return items.GroupBy(x => (x.Data as FootfetishBooruResultData)!.Tags).Select(x => (x.Key.ToList(), x.ToList()));
    }
}
