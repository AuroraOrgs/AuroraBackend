using Aurora.Application.Models;

namespace Aurora.Application.Contracts;

public interface ISearchQueryService
{
    /// <summary>
    /// Gets cached results for specified snapshots 
    /// </summary>
    /// <param name="paging"> Optional paging parameter - if none are provided, then all records are fetched</param>
    Task<SearchResults> GetResults(IEnumerable<Guid> snapshotIds, PagingOptions? paging);
}

public static class SearchQueryServiceExtensions
{
    public static Task<SearchResults> GetLatestResults(this ISearchQueryService queryService, SearchRequestState state, PagingOptions? paging)
    {
        IEnumerable<Guid> idsToLoad = state.StoredOptions
            .Values
            .Where(x => x.Snapshots.Any(x => x.IsProcessed))
            .Select(x => x.Snapshots.Where(x => x.IsProcessed).OrderBy(x => x.SnapshotTime).LastOrDefault()?.SnapshotId)
            .Where(x => x.HasValue)
            .Select(x => x!.Value);
        return queryService.GetResults(idsToLoad, paging);
    }
}