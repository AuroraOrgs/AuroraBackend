using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Extensions;
using Aurora.Shared.Extensions;
using Aurora.Shared.Models;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Scrapers
{
    public class FootFetishBooruTotalScraper : ITotalScraper
    {
        private readonly IHttpClientFactory _clientFactory;

        public FootFetishBooruTotalScraper(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public SupportedWebsite Website => SupportedWebsite.FootFetishBooru;

        public async Task<IEnumerable<(string Term, List<SearchItem> Items)>> Scrap()
        {
            var htmlDocument = new HtmlDocument
            {
                OptionFixNestedTags = true
            };

            List<(string Term, List<SearchItem> Items)> result;
            using var client = _clientFactory.CreateClient(HttpClientNames.DefaultClient);

            var baseUrl = Website.GetBaseUrl();
            var fullUrl = $"{baseUrl}/index.php?page=post&s=list";
            if (await client.TryLoadDocumentFromUrl(htmlDocument, fullUrl))
            {
                var pagesResult = FootFetishBooruImageGifScraper.ExtractPagesCount(htmlDocument);
                var pagesCount = pagesResult.WithDefault(-1);
                if (pagesCount != -1)
                {
                    Dictionary<string, List<SearchItem>> items = new();
                    for (int i = 0; i < pagesCount; i++)
                    {
                        var pageUrl = $"{fullUrl}&pid={pagesCount * FootFetishBooruImageGifScraper._itemsPerPage}";
                        if (await client.TryLoadDocumentFromUrl(htmlDocument, pageUrl))
                        {
                            var posts = htmlDocument.DocumentNode.SelectNodes("//a[@id]").Where(x => x.Id.StartsWith("p") && x.Id != "pi");
                            foreach (var post in posts)
                            {
                                var previewImage = post.ChildNodes.Where(x => x.Name == "img").FirstOrDefault();
                                var previewSrc = previewImage.GetAttributeValue("src", "none");
                                var termsStr = previewImage.GetAttributeValue("title");
                                if (termsStr is not null)
                                {
                                    var specialTerms = new string[] { "score", "rating" };
                                    var terms = termsStr.Split(" ").Select(x => x.Trim())
                                        .Where(term => specialTerms.Where(special => term.StartsWith(special)).None());

                                    var hrefValue = post.GetAttributeValue("href", "none");
                                    var location = $"{baseUrl}/{hrefValue}".Replace("&amp;", "&");
                                    SearchOption option;
                                    if (previewSrc.EndsWith("gif"))
                                    {
                                        option = SearchOption.Gif;
                                    }
                                    else
                                    {
                                        option = SearchOption.Image;
                                    }
                                    var item = new SearchItem(option, previewSrc, location);
                                    foreach (var term in terms)
                                    {
                                        items.AddList(term, item);
                                    }
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
}
