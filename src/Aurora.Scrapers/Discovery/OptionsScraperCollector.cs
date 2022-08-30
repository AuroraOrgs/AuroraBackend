using Aurora.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aurora.Scrapers.Discovery;

public class OptionsScraperCollector : IOptionsScraperCollector
{
    public IEnumerable<(SupportedWebsite Key, ContentType value)> AllowedKeys => _ctx.Scrapers.Keys;

    private readonly IServiceProvider _provider;
    private readonly OptionScraperContext _ctx;

    public OptionsScraperCollector(IServiceProvider provider, OptionScraperContext ctx)
    {
        _provider = provider;
        _ctx = ctx;
    }

    public ValueTask<IEnumerable<IOptionScraper>> CollectFor(IEnumerable<(SupportedWebsite Key, ContentType value)> keys)
    {
        var loggerFactory = _provider.GetRequiredService<ILoggerFactory>();

        var scrapers = keys.Select(key => GetScrapersOrEmpty(_ctx.Scrapers, key))
                           .Flatten()
                           .Distinct()
                           .Select(_provider.GetService)
                           .OfType<IOptionScraper>()
                           .Select(x => new OptionScraperTimeDecorator(loggerFactory, x) as IOptionScraper);

        return ValueTask.FromResult(scrapers);
    }

    private static List<TValue> GetScrapersOrEmpty<TValue>(Dictionary<(SupportedWebsite, ContentType), List<TValue>> dict, (SupportedWebsite, ContentType) key)
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
