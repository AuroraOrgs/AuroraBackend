using Aurora.Application.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Application.Scrapers
{
    public class OptionScraperTimeDecorator : IOptionScraper
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptionScraper _innerScraper;

        public OptionScraperTimeDecorator(ILoggerFactory loggerFactory, IOptionScraper innerScraper)
        {
            _loggerFactory = loggerFactory;
            _innerScraper = innerScraper;
        }

        public SupportedWebsite Website => _innerScraper.Website;

        public IEnumerable<SearchOption> Options => _innerScraper.Options;

        public async Task<List<SearchItem>> ScrapAsync(string term, CancellationToken token = default)
        {
            var currentScraper = _innerScraper.GetType();
            var logger = _loggerFactory.CreateLogger(currentScraper);
            var watch = Stopwatch.StartNew();
            try
            {
                return await _innerScraper.ScrapAsync(term, token);
            }
            finally
            {
                watch.Stop();
                var ranFor = watch.Elapsed;
                var time = ranFor.ToString();
                var scraperName = currentScraper.Name;

                logger.LogInformation("Scraper '{scraperName}' finished in '{time}'", scraperName, time);
            }
        }
    }
}
