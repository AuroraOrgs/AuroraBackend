using Aurora.Application.Models;

namespace Aurora.Application.Scrapers;

public interface IOptionScraper
{
    SupportedWebsite Website { get; }
    IEnumerable<ContentType> ContentTypes { get; }
    Task<List<SearchItem>> ScrapAsync(List<string> terms, CancellationToken token = default);
}
