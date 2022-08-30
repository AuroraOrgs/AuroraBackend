using Aurora.Application.Models;

namespace Aurora.Application.Scrapers;

public interface ITotalScraper
{
    SupportedWebsite Website { get; }
    Task<IEnumerable<(List<string> Terms, List<SearchItem> Items)>> Scrap();
}
