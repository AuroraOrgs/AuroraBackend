using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aurora.Infrastructure.Services;

public class SearchQueryService : ISearchQueryService
{
    private readonly SearchContext _context;
    private readonly ILogger<SearchQueryService> _logger;

    public SearchQueryService(SearchContext context, ILogger<SearchQueryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<SearchResults> GetResults(SearchRequestState state, PagingOptions? paging)
    {
        IEnumerable<Guid> idsToLoad = state.StoredOptions.Values
            .Where(x => x.Snapshots.Any(x => x.IsProcessed))
            .Select(x => x.Snapshots.Where(x => x.IsProcessed).OrderBy(x => x.SnapshotTime).LastOrDefault()?.SnapshotId)
            .Where(x => x.HasValue)
            .Select(x => x!.Value);
        return GetResults(idsToLoad, paging);
    }

    public async Task<SearchResults> GetResults(IEnumerable<Guid> snapshotIds, PagingOptions? paging)
    {
        SearchResults result;
        if (snapshotIds.Any())
        {
            var filteredResults = _context.Result
                .AsNoTracking()
                .Include(x => x.SearchOptionSnapshot)
                .ThenInclude(x => x.SearchOption)
                .Where(request => snapshotIds.Contains(request.SearchOptionSnapshotId));
            var query = filteredResults;
            if (paging is not null)
            {
                var toSkip = paging.PageSize * paging.PageNumber;
                query = query
                    .OrderBy(x => x.Id)
                    .Skip(toSkip)
                    .Take(paging.PageSize);
            }
            var storedResults = await query
                .ToListAsync();
            var resultToTerms = storedResults.GroupBy(x => x.SearchOptionSnapshot.SearchOption.SearchTerm);

            var results = storedResults.GroupBy(res => res.SearchOptionSnapshot)
                .Select(group => new SearchResultDto(
                    group.Select(item => new SearchItem(group.Key.SearchOption.ContentType, item.ImagePreviewUrl, item.SearchItemUrl, item.AdditionalData.ToData<SearchResultData>())).ToList(),
                    group.Key.SearchOption.SearchTerm.Terms.ToList(),
                    group.Key.SearchOption.Website))
                .ToList();
            var count = await filteredResults.CountAsync();
            _logger.LogInformation("Loaded '{resultsCount}' out of total of '{existingCount}'  existing results for requests '[{ids}]'", results.Count, count, snapshotIds.CommaSeparate());
            result = new SearchResults(results, count);
        }
        else
        {
            result = new SearchResults(new List<SearchResultDto>(), 0);
        }
        return result;
    }
}
