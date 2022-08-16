using Aurora.Application.Models;

namespace Aurora.Application.Contracts;

public interface ISearchDataService
{
    /// <summary>
    /// Gets current state for provided request
    /// </summary>
    Task<SearchRequestState> FetchRequest(SearchRequestDto request, bool isUserGenerated);
    /// <summary>
    /// Gets cached results for specified request 
    /// and information about the state of result
    /// </summary>
    /// <param name="paging"> Optional paging parameter - if none are provided, then all records are fetched</param>
    Task<SearchResults> GetResults(SearchRequestState state, PagingOptions? paging);
    /// <summary>
    /// Would mark requests items that have to be queued as queued
    /// </summary>
    Task MarkAsQueued(SearchRequestState request);
    /// <summary>
    /// Save results of the scraping
    /// </summary>
    Task AddOrUpdateResults(SearchRequestState state, IEnumerable<SearchResultDto> results);
}
