using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aurora.Application.Contracts
{
    public interface ISearchScraperCollector
    {
        ValueTask<IEnumerable<ISearchScraper>> CollectFor(List<string> websites);
    }
}
