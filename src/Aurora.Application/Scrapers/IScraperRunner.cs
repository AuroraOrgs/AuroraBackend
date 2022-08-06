using Aurora.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Application.Scrapers
{
    public interface IScraperRunner
    {
        Task<List<SearchResultDto>> Run(SearchRequestDto searchRequest, Func<SearchResultDto, Task>? onProcessed, CancellationToken token = default);
    }
}