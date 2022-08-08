using Aurora.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aurora.Application.Scrapers
{
    public interface ITotalScraper
    {
        SupportedWebsite Website { get; }
        Task<IEnumerable<(List<string> Terms, List<SearchItem> Items)>> Scrap();
    }
}
