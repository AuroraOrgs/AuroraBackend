using Aurora.Domain.Enums;
using Aurora.Scrapers.Services;

namespace Aurora.Scrapers.Option;

public class PornhubVideosScraper : IOptionScraper
{
    private readonly PagingRunner _runner;

    public PornhubVideosScraper(PagingRunner runner)
    {
        _runner = runner;
    }

    public SupportedWebsite Website => SupportedWebsite.Pornhub;
    public IEnumerable<ContentType> ContentTypes { get; init; } = new List<ContentType>() { ContentType.Video };

    public async Task<List<SearchItem>> ScrapAsync(List<string> terms, CancellationToken token = default)
    {
        var baseUrl = Website.GetBaseUrl();

        //TODO: Implement scraping of all pages
        return await _runner.RunPagingAsync(HttpClientNames.DefaultClient,
        loadPage: async (pageNumber, client) =>
        {
            var searchTerm = string.Join(" ", terms);
            var searchTermUrlFormatted = searchTerm.FormatTermToUrl();
            var searchPageUrl = $"{baseUrl}/video/search?search={searchTermUrlFormatted}&page={pageNumber + 1}";
            return await client.TryLoadDocumentFromUrl(searchPageUrl);
        },
        scrapPage: (document) =>
        {
            List<SearchItem> videoItems = new();

            var videoLinksNodes = document.DocumentNode
               ?.SelectNodes("//a[contains(@class, 'videoPreviewBg')]");

            if (videoLinksNodes is not null)
            {
                foreach (var videoLinkNode in videoLinksNodes)
                {
                    var currentLinkImageNode = videoLinkNode.ChildNodes
                        .FirstOrDefault(n => n.Name == "img");
                    //TODO: Add default image or make preview image nullable
                    string previewImage = "";

                    if (currentLinkImageNode is not null)
                    {
                        var currentLinkImageAttributes = currentLinkImageNode.Attributes;
                        previewImage = currentLinkImageAttributes["data-thumb_url"].Value;
                    }

                    var currentLinkAttributes = videoLinkNode.Attributes;
                    string itemUrl = $"{baseUrl}{currentLinkAttributes["href"].Value}";

                    videoItems.Add(new(ContentType.Video, previewImage, itemUrl));
                }
            }

            return Task.FromResult(videoItems);
        });
    }
}
