using Aurora.Application.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Aurora.Application.Scrapers
{
    public class TotalScraperTimeDecorator : ITotalScraper
    {
        private readonly ILoggerFactory _factory;
        private readonly ITotalScraper _innerScraper;

        public TotalScraperTimeDecorator(ILoggerFactory factory, ITotalScraper innerScraper)
        {
            _factory = factory;
            _innerScraper = innerScraper;
        }

        public SupportedWebsite Website => _innerScraper.Website;

        public async Task<IEnumerable<(List<string> Terms, List<SearchItem<SearchResultData>> Items)>> Scrap()
        {
            var currentScraper = _innerScraper.GetType();
            var logger = _factory.CreateLogger(currentScraper);
            var watch = Stopwatch.StartNew();
            try
            {
                return await _innerScraper.Scrap();
            }
            finally
            {
                watch.Stop();
                var ranFor = watch.Elapsed;
                var time = ranFor.ToString();
                var scraperName = currentScraper.Name;

                logger.LogInformation("Total Scraper '{scraperName}' finished in '{time}'", scraperName, time);
            }
        }
    }
}
