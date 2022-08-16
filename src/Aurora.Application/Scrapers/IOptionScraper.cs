using Aurora.Application.Models;

namespace Aurora.Application.Scrapers;

public interface IOptionScraper
{
    SupportedWebsite Website { get; }
    IEnumerable<ContentType> ContentTypes { get; }
    //TODO: Add generic type preservation
    Task<List<SearchItem<SearchResultData>>> ScrapAsync(List<string> terms, CancellationToken token = default);
}
