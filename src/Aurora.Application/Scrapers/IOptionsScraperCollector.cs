using Aurora.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aurora.Application.Scrapers
{
    public interface IOptionsScraperCollector
    {
        ValueTask<IEnumerable<IOptionScraper>> CollectFor(IEnumerable<(SupportedWebsite Key, SearchOption value)> keys);
        IEnumerable<(SupportedWebsite Key, SearchOption value)> AllowedKeys { get; }
    }
}
