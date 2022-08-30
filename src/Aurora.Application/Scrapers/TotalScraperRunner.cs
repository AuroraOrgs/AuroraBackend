using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Aurora.Application.Scrapers;

public class TotalScraperRunner : ITotalScraperRunner
{
    private readonly IServiceProvider _provider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISearchRepository _repo;

    public TotalScraperRunner(IServiceProvider provider, ILoggerFactory loggerFactory, ISearchRepository repo)
    {
        _provider = provider;
        _loggerFactory = loggerFactory;
        _repo = repo;
    }

    public async Task RunTotalScraper(Type scraperType)
    {
        if (_provider.GetService(scraperType) is ITotalScraper scraper)
        {
            var fullScraper = new TotalScraperTimeDecorator(_loggerFactory, scraper);
            var website = fullScraper.Website;
            var results = await fullScraper.Scrap();
            foreach (var result in results)
            {
                var request = new SearchRequestDto()
                {
                    Websites = new List<SupportedWebsite>()
                    {
                        website
                    },
                    ContentTypes = ContentTypeContext.ContentTypes,
                    SearchTerms = result.Terms
                };
                var resultDto = new SearchResultDto(result.Items, result.Terms, website);
                var state = await _repo.FetchRequest(request, false);
                await _repo.MarkAsQueued(state);
                await _repo.AddOrUpdateResults(state, new[] { resultDto });
            }
        }
        else
        {
            var logger = _loggerFactory.CreateLogger(scraperType);
            logger.LogError("Scraper '{type} has no implementation registered for it'", scraperType.FullName);
        }
    }
}
