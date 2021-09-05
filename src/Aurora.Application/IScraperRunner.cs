using Aurora.Application.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Application
{
    public interface IScraperRunner
    {
        Task<List<SearchResult>> Run(SearchRequest searchRequest, CancellationToken token = default);
    }
}