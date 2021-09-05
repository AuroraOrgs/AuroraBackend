using Aurora.Application.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Infrastructure
{
    public class SearchScraperCollector : ISearchScraperCollector
    {
        private static Dictionary<string, ISearchScraper> _websiteNameToScraper = new Dictionary<string, ISearchScraper>
        {
            //initialize scrapers linked to proper website
        };

        private readonly ILogger<SearchScraperCollector> _logger;
        public SearchScraperCollector(ILogger<SearchScraperCollector> logger)
        {
            _logger = logger;
        }

        public ValueTask<IEnumerable<ISearchScraper>> Collect(List<string> websites)
        {
            List<ISearchScraper> scrapers = new List<ISearchScraper>();
            foreach (var website in websites)
            {
                if (HaveScraper(website, out ISearchScraper scraper))
                {
                    scrapers.Add(scraper);
                }
                else
                {
                    _logger.LogInformation("Unable to locate scraper for requested website '{0}'", website);
                }
            }
            return ValueTask.FromResult<IEnumerable<ISearchScraper>>(scrapers);
        }

        private static bool HaveScraper(string website, out ISearchScraper scraper)
        {
            return _websiteNameToScraper.TryGetValue(website, out scraper);
        }
    }
}
