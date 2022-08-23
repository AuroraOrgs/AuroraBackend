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

    public async Task<List<SearchItem<SearchResultData>>> ScrapAsync(List<string> terms, CancellationToken token = default)
    {
        string term = string.Join("+", terms.Select(TermToUrlFormat));
        return await _runner.RunPagingAsync(HttpClientNames.DefaultClient,
            loadPage: (pageNumber, client) => LoadPage(term, pageNumber, client),
            scrapPage: document =>
            {
                var posts = document.DocumentNode.SelectNodes("//a[@id]")
                                                     .Where<HtmlNode>(x => x.Id.StartsWith("p") && x.Id != "pi");
                List<SearchItem<SearchResultData>> items = new();
                foreach (var post in posts)
                {
                    var hrefValue = post.GetAttributeValue("href", "none");
                    var location = $"{Website.GetBaseUrl()}/{hrefValue}".Replace("&amp;", "&");
                    var previewImage = post.ChildNodes.Where<HtmlNode>(x => x.Name == "img").First();
                    var previewSrc = previewImage.GetAttributeValue("src", "none");

                    ContentType type;
                    if (previewSrc.EndsWith("gif"))
                    {
                        type = ContentType.Gif;
                    }
                    else
                    {
                        type = ContentType.Image;
                    }
                    var termsStr = previewImage.GetAttributeValue("title");
                    SearchItem<SearchResultData> item;
                    if (termsStr is not null)
                    {
                        var specialTerms = new string[] { "score", "rating" };
                        var terms = termsStr.Split(" ").Select(x => x.Trim())
                            .Where(term => specialTerms.Where(special => term.StartsWith(special)).None())
                            .Where(term => term.IsNotEmpty())
                            .ToArray();
                        item = new SearchItem<SearchResultData>(type, previewSrc, location, new FootfetishBooruResultData(terms));
                    }
                    else
                    {
                        item = new SearchItem<SearchResultData>(type, previewSrc, location);
                    }
                    items.Add(item);
                }
                return Task.FromResult(items);
            },
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
