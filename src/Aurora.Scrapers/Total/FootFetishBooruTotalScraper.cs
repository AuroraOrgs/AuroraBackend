using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Scrapers.Config;
using Aurora.Scrapers.Contracts;
using Aurora.Scrapers.Extensions;
using Aurora.Scrapers.Option;
using Aurora.Shared.Extensions;
using Aurora.Shared.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;

namespace Aurora.Scrapers.Total;

public class FootFetishBooruTotalScraper : ITotalScraper
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IOptions<TotalScrapersConfig> _options;

    public FootFetishBooruTotalScraper(IHttpClientFactory clientFactory, IOptions<TotalScrapersConfig> options)
    {
        _clientFactory = clientFactory;
        _options = options;
    }

    public SupportedWebsite Website => SupportedWebsite.FootFetishBooru;

    public async Task<IEnumerable<(List<string> Terms, List<SearchItem<SearchResultData>> Items)>> Scrap()
    {
        var config = _options.Value;
        var htmlDocument = new HtmlDocument
        {
            OptionFixNestedTags = true
        };

        List<(List<string> Terms, List<SearchItem<SearchResultData>> Items)> result;
        using var client = _clientFactory.CreateClient(HttpClientNames.DefaultClient);

        var baseUrl = Website.GetBaseUrl();
        var fullUrl = $"{baseUrl}/index.php?page=post&s=list";
        if (await client.TryLoadDocumentFromUrl(htmlDocument, fullUrl))
        {
            var pagesResult = FootFetishBooruImageGifScraper.ExtractPagesCount(htmlDocument);
            var pagesCount = pagesResult.WithDefault(-1);
            if (pagesCount != -1)
            {
                if (config.UseLimitations)
                {
                    pagesCount = Math.Min(config.MaxPagesCount, pagesCount);
                }
                Dictionary<List<string>, List<SearchItem<SearchResultData>>> items = new();
                for (int i = 0; i < pagesCount; i++)
                {
                    var pageUrl = $"{fullUrl}&pid={i * ScraperConstants.FootFetishBooruPostsPerPage}";
                    if (await client.TryLoadDocumentFromUrl(htmlDocument, pageUrl))
                    {
                        var posts = htmlDocument.DocumentNode.SelectNodes("//a[@id]").Where(x => x.Id.StartsWith("p") && x.Id != "pi");
                        foreach (var post in posts)
                        {
                            var previewImage = post.ChildNodes.Where(x => x.Name == "img").First();
                            var previewSrc = previewImage.GetAttributeValue("src", "none");
                            var termsStr = previewImage.GetAttributeValue("title");
                            if (termsStr is not null)
                            {
                                var specialTerms = new string[] { "score", "rating" };
                                var terms = termsStr.Split(" ").Select(x => x.Trim())
                                    .Where(term => specialTerms.Where(special => term.StartsWith(special)).None())
                                    .Where(term => term.IsNotEmpty())
                                    .ToList();

                                var hrefValue = post.GetAttributeValue("href", "none");
                                var location = $"{baseUrl}/{hrefValue}".Replace("&amp;", "&");
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
                                items.AddList(terms, item);
                            }
                        }
                    }
                }
                result = items.Select(pair => (pair.Key, pair.Value)).ToList();
            }
            else
            {
                result = new();
            }
        }
        else
        {
            result = new();
        }
        return result;
    }
}
