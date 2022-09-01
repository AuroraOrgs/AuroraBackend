using Aurora.Application.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Aurora.Application.Scrapers;

public class ScraperRunner : IScraperRunner
{
    //Used to prevent excessive allocation
    private static readonly List<SearchItem> _emptyList = new();
    //TODO: Add configuration of maximum semaphore count
    private Semaphore _scraperLimiter = new Semaphore(1, 5);
    private readonly IOptionsScraperCollector _collector;
    private readonly ILogger<ScraperRunner> _logger;

    public ScraperRunner(IOptionsScraperCollector collector, ILogger<ScraperRunner> logger)
    {
        _collector = collector;
        _logger = logger;
    }

    public async Task<List<SearchResultDto>> RunAsync(SearchRequestDto searchRequest, Func<SearchResultDto, Task>? onProcessed, CancellationToken token = default)
    {
        var options = searchRequest.Websites.Select(website => searchRequest.ContentTypes.Select(option => (website, option))).Flatten();
        var scrapers = await _collector.CollectFor(options);
        IEnumerable<Task<(ValueOrNull<List<SearchItem>> result, IOptionScraper scraper)>> scrapingTasks = null!;
        List<SearchResultDto> result;
        try
        {
            scrapingTasks = scrapers.Select(scraper => ExecuteScraping(scraper, searchRequest.SearchTerms, onProcessed, token));
            var results = await Task.WhenAll(scrapingTasks);
            result = ProcessResults(results, searchRequest.SearchTerms);
        }
        catch (OperationCanceledException)
        {
            //if cancelled - process processed records
            if (scrapingTasks is not null)
            {
                var results = scrapingTasks.Where(x => x.IsCompletedSuccessfully).Select(x => x.Result);
                //TODO: update scraping process to scrap multiple tags and store them instead of reusing tags from request
                result = ProcessResults(results, searchRequest.SearchTerms);
            }
            else
            {
                result = new List<SearchResultDto>();
            }
        }
        return result;
    }

    private static List<SearchResultDto> ProcessResults(IEnumerable<(ValueOrNull<List<SearchItem>> result, IOptionScraper scraper)> results, List<string> terms)
    {
        return results.Select(x => (x.scraper, items: x.result.WithDefault(_emptyList)))
                      .GroupBy(x => x.scraper.Website)
                      .Select(x => new SearchResultDto(x.Select(x => x.items).Flatten().ToList(), terms, x.Key))
                      .ToList();
    }

    private async Task<(ValueOrNull<List<SearchItem>> result, IOptionScraper scraper)> ExecuteScraping(IOptionScraper scraper, List<string> terms, Func<SearchResultDto, Task>? onProcessed, CancellationToken token)
    {
        ValueOrNull<List<SearchItem>> result;
        try
        {
            _scraperLimiter.WaitOne();
            result = await scraper.ScrapAsync(terms, token);
            onProcessed?.Invoke(new SearchResultDto(result.WithDefault(_emptyList), terms, scraper.Website));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scrap with following message '{0}'", ex.Message);
            result = ValueOrNull<List<SearchItem>>.CreateNull($"Failed to scrap with {ex.Message}");
        }
        finally
        {
            _scraperLimiter.Release();
        }
        return (result, scraper);
    }
}