using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Services
{
    public class OptionsScraperCollector : IOptionsScraperCollector
    {
        private static Dictionary<(SupportedWebsite, SearchOption), List<Type>> _scrapers = null;
        private static List<Type> _totalScrapers = null;
        public static List<Type> TotalScrapers => _totalScrapers;

        public IEnumerable<(SupportedWebsite Key, SearchOption value)> AllowedKeys => _scrapers.Keys;

        private readonly IServiceProvider _provider;


        public OptionsScraperCollector(IServiceProvider provider)
        {
            _provider = provider;
        }

        public static (IEnumerable<Type> OptionScrapers, IEnumerable<Type> TotalScrapers) DiscoverScrapers(IServiceCollection services)
        {
            Dictionary<(SupportedWebsite, SearchOption), List<Type>> scrapers = new();
            var baseScraperType = typeof(IOptionScraper);
            var baseTotalScraperType = typeof(ITotalScraper);
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsAssignableTo(baseScraperType) || x.IsAssignableTo(baseTotalScraperType));
            var provider = services.BuildServiceProvider();
            var optionScrapers = new List<Type>();
            var totalScrapers = new List<Type>();
            foreach (var type in types)
            {
                if (type.IsAssignableTo(baseScraperType))
                {
                    var instance = ActivatorUtilities.CreateInstance(provider, type) as IOptionScraper;
                    foreach (var option in instance.Options)
                    {
                        var key = (instance.Website, option);
                        scrapers.AddList(key, type);
                    }
                    optionScrapers.Add(type);
                }
                else
                {
                    totalScrapers.Add(type);
                }
            }

            _scrapers = scrapers;
            _totalScrapers = totalScrapers;

            return (optionScrapers, totalScrapers);
        }

        public ValueTask<IEnumerable<IOptionScraper>> CollectFor(IEnumerable<(SupportedWebsite Key, SearchOption value)> keys)
        {
            var loggerFactory = _provider.GetRequiredService<ILoggerFactory>();

            var scrapers = keys.Select(key => GetScrapersOrEmpty(_scrapers, key))
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
