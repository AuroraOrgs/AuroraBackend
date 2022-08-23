using Aurora.Scrapers.Services;

namespace Aurora.Scrapers.Option;

public class XVideosVideosScraper : IOptionScraper
{
    private readonly PagingRunner _runner;

    public XVideosVideosScraper(PagingRunner runner)
    {
        _runner = runner;
    }

    public SupportedWebsite Website => SupportedWebsite.XVideos;
    public IEnumerable<ContentType> ContentTypes { get; init; } = new List<ContentType>() { ContentType.Video };

    public async Task<List<SearchItem<SearchResultData>>> ScrapAsync(List<string> terms, CancellationToken token = default)
    {
        var baseUrl = Website.GetBaseUrl();
        var term = string.Join(" ", terms);
        var searchTermUrlFormatted = term.FormatTermToUrl();

        //TODO: Implement scraping of all pages
        return await _runner.RunPagingAsync(HttpClientNames.XVideosClient,
            loadPage: async (pageNumber, client) =>
            {
                var searchPageUrl = $"{baseUrl}/?k={searchTermUrlFormatted}&p={pageNumber}";
                return await client.TryLoadDocumentFromUrl(searchPageUrl);
            },
            scrapPage: document =>
            {
                List<SearchItem<SearchResultData>> videoItems = new();
                var videoLinksNodes = document.DocumentNode
                ?.SelectNodes("//a");

                if (videoLinksNodes is not null)
                {
                    foreach (var videoLinkNode in videoLinksNodes)
                    {
                        var currentLinkImageNode = videoLinkNode.ChildNodes
                            .FirstOrDefault(n => n.Name == "img");

                        if (currentLinkImageNode is not null)
                        {
                            var currentLinkImageAttributes = currentLinkImageNode.Attributes;
                            string imagePreviewUrl = currentLinkImageAttributes["data-src"]?.Value ?? "";
                            if (imagePreviewUrl is not null)
                            {
                                var currentLinkAttributes = videoLinkNode.Attributes;
                                var videoLink = currentLinkAttributes["href"]?.Value;
                                if (videoLink is not null && videoLink.Contains("video") && !videoLink.Contains("videos"))
                                {
                                    string searchItemUrl = $"{baseUrl}{videoLink}";
                                    if (searchItemUrl is not null && imagePreviewUrl is not null)
                                    {
                                        videoItems.Add(new(ContentType.Video, imagePreviewUrl, searchItemUrl));
                                    }
                                }
                            }
                        }
                    }
                }
                return Task.FromResult(videoItems);
            });
    }
}
