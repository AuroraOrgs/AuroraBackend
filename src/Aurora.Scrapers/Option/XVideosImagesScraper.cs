using Aurora.Domain.Enums;
using Aurora.Scrapers.Services;

namespace Aurora.Scrapers.Option;

public class XVideosImagesScraper : IOptionScraper
{
    private readonly PagingRunner _runner;

    public XVideosImagesScraper(PagingRunner runner)
    {
        _runner = runner;
    }

    public SupportedWebsite Website => SupportedWebsite.XVideos;
    public IEnumerable<ContentType> ContentTypes { get; init; } = new List<ContentType> { ContentType.Image };

    public async Task<List<SearchItem>> ScrapAsync(List<string> terms, CancellationToken token = default)
    {
        var baseUrl = Website.GetBaseUrl();
        var term = string.Join(" ", terms);
        var searchTermUrlFormatted = term.FormatTermToUrl();
        //TODO: Implement scraping of all pages
        return await _runner.RunPagingAsync(HttpClientNames.XVideosClient,
            loadPage: async (pageNumber, client) =>
            {
                var searchPageUrl = $"{baseUrl}/?k={searchTermUrlFormatted}&p={pageNumber + 1}";
                return await client.TryLoadDocumentFromUrl(searchPageUrl);
            },
            scrapPage: (document) =>
            {
                List<SearchItem> imageItems = new();
                var videoLinksNodes = document.DocumentNode?.SelectNodes("//a");

                if (videoLinksNodes is not null)
                {
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

                    }
                }
                return Task.FromResult(imageItems);
            }
            );
    }
}
