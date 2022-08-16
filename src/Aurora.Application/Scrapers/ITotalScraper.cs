using Aurora.Application.Models;

namespace Aurora.Application.Scrapers;

public interface ITotalScraper
{
    SupportedWebsite Website { get; }
    //TODO: Add generic type preservation
    Task<IEnumerable<(List<string> Terms, List<SearchItem<SearchResultData>> Items)>> Scrap();
}
