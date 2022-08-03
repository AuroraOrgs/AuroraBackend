using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Scrapers
{
    public class FootFetishBooruTotalScraper : ITotalScraper
    {
        public SupportedWebsite Website => SupportedWebsite.FootFetishBooru;

        public async Task<IEnumerable<(string Term, List<SearchItem> Items)>> Scrap()
        {
            List<(string Term, List<SearchItem> Items)> result = new();
            result.Add(("Test", new List<SearchItem>()
            {
                new SearchItem(SearchOption.Video, "abcs", "dsa"),
                new SearchItem(SearchOption.Video, "dsads", "asasa")
            }));
            return result;
        }
    }
}
