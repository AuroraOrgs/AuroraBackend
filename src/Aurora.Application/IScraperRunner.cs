using Aurora.Application.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Application
{
    public interface IScraperRunner
    {
        Task<List<SearchResultDto>> Run(SearchRequestDto searchRequest, CancellationToken token = default);
    }
}