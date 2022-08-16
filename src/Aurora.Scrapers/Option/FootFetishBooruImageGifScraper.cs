using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Scrapers.Config;
using Aurora.Scrapers.Contracts;
using Aurora.Scrapers.Extensions;
using Aurora.Shared.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;

namespace Aurora.Scrapers.Option;

public class FootFetishBooruImageGifScraper : IOptionScraper
{
    private static List<SearchItem<SearchResultData>> _emptyResult = new List<SearchItem<SearchResultData>>();

    private readonly IHttpClientFactory _clientFactory;
    private readonly IOptions<ScrapersConfig> _config;

    public FootFetishBooruImageGifScraper(IHttpClientFactory clientFactory, IOptions<ScrapersConfig> config)
    {
        _clientFactory = clientFactory;
        _config = config;
    }

    public SupportedWebsite Website => SupportedWebsite.FootFetishBooru;
    public IEnumerable<ContentType> ContentTypes { get; init; } = new List<ContentType> { ContentType.Image, ContentType.Gif };

    public async Task<List<SearchItem<SearchResultData>>> ScrapAsync(List<string> terms, CancellationToken token = default)
    {
        var config = _config.Value;
        using var client = _clientFactory.CreateClient(HttpClientNames.DefaultClient);
        var baseUrl = Website.GetBaseUrl();
        string term = string.Join("+", terms.Select(TermToUrlFormat));
        var fullUrl = $"{baseUrl}/index.php?page=post&s=list&tags={term}";
        var htmlDocument = new HtmlDocument
        {
            OptionFixNestedTags = true
        };

        List<SearchItem<SearchResultData>> result;
        if (await client.TryLoadDocumentFromUrl(htmlDocument, fullUrl))
        {
            var pageResult = ExtractPagesCount(htmlDocument);
            result = await pageResult.ResolveAsync(async pagesCount =>
            {
                int lastPageIndex;
                if (config.UseLimitations)
                {
                    lastPageIndex = Math.Min(pagesCount, config.MaxPagesCount);
                }
                else
                {
                    lastPageIndex = pagesCount;
                }
                var items = new List<SearchItem<SearchResultData>>();
                for (int i = 0; i < lastPageIndex; i++)
                {
                    items.AddRange(await LoadPageAsync(term, client, i));
                    if (config.UseLimitations && items.Count > config.MaxItemsCount)
                    {
                        break;
                    }
                }
                return items;
            },
            _ => Task.FromResult(_emptyResult));
        }
        else
        {
            result = _emptyResult;
        }
        return result;
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

    private async Task<List<SearchItem<SearchResultData>>> LoadPageAsync(string term, HttpClient client, int pageNumber)
    {
        var htmlDocument = new HtmlDocument
        {
            OptionFixNestedTags = true
        };
        var baseUrl = Website.GetBaseUrl();
        var items = new List<SearchItem<SearchResultData>>();
        var pageUrl = $"{baseUrl}/index.php?page=post&s=list&tags={term}&pid={pageNumber * ScraperConstants.FootFetishBooruPostsPerPage}";
        if (await client.TryLoadDocumentFromUrl(htmlDocument, pageUrl))
        {
            var posts = htmlDocument.DocumentNode.SelectNodes("//a[@id]")
                .Where(x => x.Id.StartsWith("p") && x.Id != "pi");
            foreach (var post in posts)
            {
                var hrefValue = post.GetAttributeValue("href", "none");
                var location = $"{baseUrl}/{hrefValue}".Replace("&amp;", "&");
                var previewImage = post.ChildNodes.Where(x => x.Name == "img").First();
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
                var item = new SearchItem<SearchResultData>(type, previewSrc, location);
                items.Add(item);
            }
        }
        return items;
    }

    private static string TermToUrlFormat(string term) =>
        term.Replace(" ", "_");
}
