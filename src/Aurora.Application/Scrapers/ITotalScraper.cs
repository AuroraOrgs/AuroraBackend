using Aurora.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aurora.Application.Scrapers
{
    public interface ITotalScraper
    {
        SupportedWebsite Website { get; }
        //TODO: Add generic type preservation
        Task<IEnumerable<(List<string> Terms, List<SearchItem<SearchResultData>> Items)>> Scrap();
    }
}
