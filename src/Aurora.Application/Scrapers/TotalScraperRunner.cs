using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aurora.Application.Scrapers
{
    public class TotalScraperRunner : ITotalScraperRunner
    {
        private readonly IServiceProvider _provider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ISearchDataService _search;

        public TotalScraperRunner(IServiceProvider provider, ILoggerFactory loggerFactory, ISearchDataService search)
        {
            _provider = provider;
            _loggerFactory = loggerFactory;
            _search = search;
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
                    var state = await _search.FetchRequest(request, false);
                    await _search.MarkAsQueued(state);
                    await _search.AddOrUpdateResults(state, new[] { resultDto });
                }
            }
            else
            {
                var logger = _loggerFactory.CreateLogger(scraperType);
                logger.LogError("Scraper '{type} has no implementation registered for it'", scraperType.FullName);
            }
        }
    }
}
