using System.Collections.Generic;
using System.Threading.Tasks;
using Aurora.Application.Enums;

namespace Aurora.Application.Contracts
{
    public interface ISearchScraperCollector
    {
        ValueTask<IEnumerable<ISearchScraper>> CollectFor(List<SupportedWebsite> websites);
    }
}
