using Aurora.Application.Models;

namespace Aurora.Application.Scrapers;

public interface IScraperRunner
{
    Task<List<SearchResultDto>> RunAsync(SearchRequestDto searchRequest, Func<SearchResultDto, Task>? onProcessed, CancellationToken token = default);
}