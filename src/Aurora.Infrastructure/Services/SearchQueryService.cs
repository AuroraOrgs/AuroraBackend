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

    public async Task<SearchResults> GetResults(SearchRequestState state, PagingOptions? paging)
    {
        var idsToLoad = state.StoredOptions.Values
            .Where(x => x.Snapshots.Any(x => x.IsProcessed))
            //getting last snapshot does not work for items that were fetched before, but are currently in the process of refreshing
            //TODO:fix
            .Select(x => x.Snapshots.OrderBy(x => x.SnapshotTime).LastOrDefault()?.SnapshotId);
        SearchResults result;
        if (idsToLoad.Any())
        {
            var filteredResults = _context.Result
                .AsNoTracking()
                .Include(x => x.SearchOptionSnapshot)
                .ThenInclude(x => x.SearchOption)
                .Where(request => idsToLoad.Contains(request.SearchOptionSnapshotId));
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

            var terms = state.StoredOptions.Keys.SelectMany(x => x.Term.Terms).Distinct().ToList();

            var results = storedResults.GroupBy(res => res.SearchOptionSnapshot)
                .Select(group => new SearchResultDto(
                    group.Select(item => new SearchItem(group.Key.SearchOption.ContentType, item.ImagePreviewUrl, item.SearchItemUrl, item.AdditionalData.ToData<SearchResultData>())).ToList(),
                    terms,
                    group.Key.SearchOption.Website))
                .ToList();
            var count = await filteredResults.CountAsync();
            _logger.LogInformation("Loaded '{resultsCount}' out of total of '{existingCount}'  existing results for requests '[{ids}]'", results.Count, count, idsToLoad.CommaSeparate());
            result = new SearchResults(results, count);
        }
        else
        {
            result = new SearchResults(new List<SearchResultDto>(), 0);
        }
        return result;
    }
}
