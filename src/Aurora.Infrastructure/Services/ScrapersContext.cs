using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aurora.Infrastructure.Services
{
    public class ScrapersContext
    {
        private static Dictionary<(SupportedWebsite, ContentType), List<Type>> _scrapers;
        public static Dictionary<(SupportedWebsite, ContentType), List<Type>> Scrapers => _scrapers;

        private static List<Type> _totalScrapers;
        public static List<Type> TotalScrapers => _totalScrapers;

        public static (IEnumerable<Type> OptionScrapers, IEnumerable<Type> TotalScrapers) DiscoverScrapers(IServiceCollection services)
        {
            Dictionary<(SupportedWebsite, ContentType), List<Type>> scrapers = new();
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
                    foreach (var contentType in instance.ContentTypes)
                    {
                        var key = (instance.Website, contentType);
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
    }
}
