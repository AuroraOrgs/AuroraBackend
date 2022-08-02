using Aurora.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aurora.Application.Scrapers
{
    public interface ITotalScraper
    {
        SupportedWebsite Website { get; }
        IEnumerable<SearchOption> Options { get; }
        Task<(string Term, List<SearchItem> Items)> Scrap();
    }
}
