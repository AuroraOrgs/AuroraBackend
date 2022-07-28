using Aurora.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aurora.Application.Contracts
{
    public interface IOptionsScraperCollector
    {
        ValueTask<IEnumerable<IOptionScraper>> CollectFor(IEnumerable<(SupportedWebsite Key, SearchOption value)> keys);
    }
}
