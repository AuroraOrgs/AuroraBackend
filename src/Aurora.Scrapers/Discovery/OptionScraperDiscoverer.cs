using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Aurora.Scrapers.Discovery
{
    public class OptionScraperContext
    {
        private Dictionary<(SupportedWebsite, ContentType), List<Type>> _scrapers;

        public OptionScraperContext(IServiceScopeFactory scopeFactory)
        {
            Dictionary<(SupportedWebsite, ContentType), List<Type>> scrapers = new();
            using (var scope = scopeFactory.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var ctx = provider.GetRequiredService<ScrapersContext>();
                foreach (var type in ctx.OptionScrapers)
                {
                    var instance = ActivatorUtilities.CreateInstance(provider, type) as IOptionScraper;
                    foreach (var contentType in instance.ContentTypes)
                    {
                        var key = (instance.Website, contentType);
                        scrapers.AddList(key, type);
                    }
                }
            }
            _scrapers = scrapers;
        }

        public Dictionary<(SupportedWebsite, ContentType), List<Type>> Scrapers => _scrapers;
    }
}
