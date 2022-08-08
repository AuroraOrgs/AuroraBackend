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

        public IEnumerable<ContentType> ContentTypes => _innerScraper.ContentTypes;

        public async Task<List<SearchItem>> ScrapAsync(List<string> terms, CancellationToken token = default)
        {
            var currentScraper = _innerScraper.GetType();
            var logger = _loggerFactory.CreateLogger(currentScraper);
            var watch = Stopwatch.StartNew();
            try
            {
                return await _innerScraper.ScrapAsync(terms, token);
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
