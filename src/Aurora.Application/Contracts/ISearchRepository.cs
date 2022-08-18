using Aurora.Application.Models;

namespace Aurora.Application.Contracts;

public interface ISearchRepository
{
    /// <summary>
    /// Gets current state for provided request, storing it if it does not yet exist
    /// </summary>
    Task<SearchRequestState> FetchRequest(SearchRequestDto request, bool isUserGenerated);
    /// <summary>
    /// Would mark requests items that have to be queued as queued
    /// </summary>
    Task MarkAsQueued(SearchRequestState request);
    /// <summary>
    /// Save results of the scraping
    /// </summary>
    Task AddOrUpdateResults(SearchRequestState state, IEnumerable<SearchResultDto> results);
}
