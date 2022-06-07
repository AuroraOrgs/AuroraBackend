using Aurora.Application.Commands;
using Aurora.Application.Enums;
using Aurora.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aurora.Application.Contracts
{
    public interface ISearchDataService
    {
        /// <summary>
        /// Gets cached results for specified request 
        /// and information about the state of result
        /// </summary>
        /// <param name="paging"> Optional paging parameter - if none are provided, then all records are fetched</param>
        Task<SearchResults> GetResults(SearchRequestDto request, PagingOptions? paging);
        Task StoreRequest(SearchRequestDto request);
        Task AddOrUpdateResults(SearchRequestDto request, IEnumerable<SearchResultDto> results);
    }

    public record SearchResults(List<SearchResultDto> Results, long TotalItems, List<SupportedWebsite> ProcessedWebsites);
}
