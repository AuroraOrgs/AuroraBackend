using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Services
{
    public class OptionsScraperCollector : IOptionsScraperCollector
    {
        public IEnumerable<(SupportedWebsite Key, SearchOption value)> AllowedKeys => ScrapersContext.Scrapers.Keys;

        private readonly IServiceProvider _provider;

        public OptionsScraperCollector(IServiceProvider provider)
        {
            _provider = provider;
        }

        public ValueTask<IEnumerable<IOptionScraper>> CollectFor(IEnumerable<(SupportedWebsite Key, SearchOption value)> keys)
        {
            var loggerFactory = _provider.GetRequiredService<ILoggerFactory>();

            var scrapers = keys.Select(key => GetScrapersOrEmpty(ScrapersContext.Scrapers, key))
                               .Flatten()
                               .Distinct()
                               .Select(_provider.GetService)
                               .OfType<IOptionScraper>()
                               .Select(x => new OptionScraperTimeDecorator(loggerFactory, x) as IOptionScraper);

            return ValueTask.FromResult(scrapers);
        }

        private static List<TValue> GetScrapersOrEmpty<TKey, TValue>(Dictionary<TKey, List<TValue>> dict, TKey key)
        {
            List<TValue> result;
            if (dict.TryGetValue(key, out var value))
            {
                result = value;
            }
            else
            {
                result = new List<TValue>();
            }
            return result;
        }
    }
}
