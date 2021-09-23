using Aurora.Application.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aurora.Application.Enums;
using Aurora.Infrastructure.Scrapers;

namespace Aurora.Infrastructure
{
    public class SearchScraperCollector : ISearchScraperCollector
    {
        private static readonly Dictionary<SupportedWebsite, Type> _websiteNameToScraperType = new()
        {
            { SupportedWebsite.Pornhub, typeof(PornhubScraper) },
            { SupportedWebsite.Xvideos, typeof(XvideosScraper) }
        };

        private readonly ILogger<SearchScraperCollector> _logger;
        private readonly IServiceProvider _provider;

        public SearchScraperCollector(ILogger<SearchScraperCollector> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        public ValueTask<IEnumerable<ISearchScraper>> CollectFor(List<SupportedWebsite> websites)
        {
            List<ISearchScraper> scrapers = new();
            foreach (var website in websites)
            {
                if (HaveScraper(website, out var scraperType))
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

        private static bool HaveScraper(SupportedWebsite website, out Type scraper)
        {
            return _websiteNameToScraperType.TryGetValue(website, out scraper);
        }
    }
}
