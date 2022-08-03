using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    var options = result.Items.Select(x => x.Option).Distinct().ToList();
                    var request = new SearchRequestDto()
                    {
                        Websites = new List<SupportedWebsite>()
                        {
                            website
                        },
                        SearchOptions = options,
                        SearchTerm = result.Term
                    };
                    var resultDto = new SearchResultDto(result.Items, website);
                    var state = await _search.FetchRequest(request, false);
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
