using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                var logger = provider.GetRequiredService<ILogger<OptionScraperContext>>();
                var ctx = provider.GetRequiredService<ScrapersContext>();
                logger.LogInformation("Configuring options scrapers for '{types}'", ctx.OptionScrapers.Select(x=>x.Name).CommaSeparate());
                foreach (var type in ctx.OptionScrapers)
                {
                    if (ActivatorUtilities.CreateInstance(provider, type) is IOptionScraper instance)
                    {
                        foreach (var contentType in instance.ContentTypes)
                        {
                            var key = (instance.Website, contentType);
                            scrapers.AddList(key, type);
                        }
                    }
                    else
                    {
                        logger.LogWarning("Scraper type '{type}' cannot be initialized", type.FullName);
                    }
                }
                logger.LogInformation("Configured option scrapers");
            }
            _scrapers = scrapers;
        }

        public Dictionary<(SupportedWebsite, ContentType), List<Type>> Scrapers => _scrapers;
    }
}
