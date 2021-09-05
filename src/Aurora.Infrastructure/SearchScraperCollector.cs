using Aurora.Application.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aurora.Infrastructure
{
    public class SearchScraperCollector : ISearchScraperCollector
    {
        private static Dictionary<string, Type> _websiteNameToScraperType = new Dictionary<string, Type>
        {
            //initialize scraper type linked to proper website
        };

        private readonly ILogger<SearchScraperCollector> _logger;
        private readonly IServiceProvider _provider;

        public SearchScraperCollector(ILogger<SearchScraperCollector> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        public ValueTask<IEnumerable<ISearchScraper>> CollectFor(List<string> websites)
        {
            List<ISearchScraper> scrapers = new List<ISearchScraper>();
            foreach (var website in websites)
            {
                if (HaveScraper(website, out Type scraperType))
                {
                    var result = _provider.GetService(scraperType);
                    if (result is ISearchScraper scraper)
                    {
                        scrapers.Add(scraper);
                    }
                }
                else
                {
                    _logger.LogInformation("Unable to locate scraper for requested website '{0}'", website);
                }
            }
            return ValueTask.FromResult<IEnumerable<ISearchScraper>>(scrapers);
        }

        private static bool HaveScraper(string website, out Type scraper)
        {
            return _websiteNameToScraperType.TryGetValue(website, out scraper);
        }
    }
}
