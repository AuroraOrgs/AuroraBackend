using Aurora.Application.Models;

namespace Aurora.Application.Contracts
{
    public interface ISearchQueryService
    {
        /// <summary>
        /// Gets cached results for specified request 
        /// and information about the state of result
        /// </summary>
        /// <param name="paging"> Optional paging parameter - if none are provided, then all records are fetched</param>
        Task<SearchResults> GetResults(SearchRequestState state, PagingOptions? paging);
        Task<SearchResults> GetResults(IEnumerable<Guid> snapshotIds, PagingOptions? paging);
    }
}
