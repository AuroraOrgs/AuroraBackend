using Aurora.Application.Models;
using Aurora.Domain.Enums;

namespace Aurora.Application.Scrapers;

public interface ITotalScraper
{
    SupportedWebsite Website { get; }
    Task<IEnumerable<(List<string> Terms, List<SearchItem> Items)>> Scrap();
}
