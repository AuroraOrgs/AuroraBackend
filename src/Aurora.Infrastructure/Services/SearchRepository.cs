using Aurora.Application.Contracts;
using Aurora.Application.Entities;
using Aurora.Application.Models;
using Aurora.Application.ValueObjects;
using Aurora.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aurora.Infrastructure.Services;

public class SearchRepository : ISearchRepository
{
    private readonly SearchContext _context;
    private readonly ILogger<SearchRepository> _logger;
    private readonly IDateTimeProvider _dateTime;

    public SearchRepository(SearchContext context, ILogger<SearchRepository> logger, IDateTimeProvider dateTime)
    {
        _context = context;
        _logger = logger;
        _dateTime = dateTime;
    }

    private static SearchRequestStatus GetStatusFor(SearchRequest request)
    {
        SearchRequestStatus status;
        if (request.QueueItems.None())
        {
            status = SearchRequestStatus.NotFetched;
        }
        else
        {
            //we might want to use time of queue items to create more informed statuses for snapshots
            if (request.QueueItems.Any(x => x.IsProcessed))
            {
                status = SearchRequestStatus.Fetched;
            }
            else
            {
                status = SearchRequestStatus.Queued;
            }
        }
        return status;
    }

    public async Task<SearchRequestState> FetchRequest(SearchRequestDto request, bool isUserGenerated)
    {
        var term = SearchRequestTerm.CreateAnd(request.SearchTerms);
        var storedOptions = await _context.Request
                        .Include(x => x.QueueItems)
                        .Include(x => x.Snapshots)
                        .Where(x =>
                            request.ContentTypes.Contains(x.ContentType)
                            && request.Websites.Contains(x.Website)
                            && x.SearchTerm == term)
                        .ToListAsync();

        var existingOptions = storedOptions.Select(x => new SearchRequestOptionDto(x.Website, x.ContentType, x.SearchTerm));
        List<SearchRequestOptionDto> requestedOptions = new();
        foreach (var website in request.Websites)
        {
            foreach (var option in request.ContentTypes)
            {
                requestedOptions.Add(new(website, option, term));
            }
        }
        var newOptionModels = requestedOptions.Where(x => existingOptions.NotContains(x));
        List<SearchRequest> newOptions = CreateOptions(newOptionModels);

        if (isUserGenerated)
        {
            foreach (var existingRequest in storedOptions)
            {
                existingRequest.OccurredCount++;
            }
        }
        _context.Request.UpdateRange(storedOptions);

        var updatedCount = await _context.SaveChangesAsync();
        _logger.LogInformation("Updated '{number}' records whilst fetching", updatedCount);
        var allOptions = storedOptions.Union(newOptions).ToList();
        var result = allOptions.ToDictionary(
            key => new SearchRequestOptionDto(key.Website, key.ContentType, key.SearchTerm),
            value => new SearchRequestItem(
                value.Id,
                GetStatusFor(value),
                value.Snapshots.Select(snapshot => new SearchSnapshot(snapshot.Id, snapshot.Time)).ToList()
              ));
        return new SearchRequestState(result);
    }

    private List<SearchRequest> CreateOptions(IEnumerable<SearchRequestOptionDto> newOptions)
    {
        _logger.LogInformation(
                      "Creating new requests with '{0}' contentTypes, '{1}' websites and '{2} terms'",
                      newOptions.Select(x => x.ContentType).CommaSeparate(),
                      newOptions.Select(x => x.Website).CommaSeparate(),
                      newOptions.Select(x => x.Term.ToString()).CommaSeparate());
        var newRequests = newOptions.Select(newOption => new SearchRequest()
        {
            ContentType = newOption.ContentType,
            OccurredCount = 1,
            SearchTerm = newOption.Term,
            Website = newOption.Website
        }).ToList();
        _context.Request
            .AddRange(newRequests);
        return newRequests;
    }

    public async Task MarkAsQueued(SearchRequestState request)
    {
        var requestIds = request.StoredOptions.Values.Where(x => x.RequestStatus == SearchRequestStatus.NotFetched).Select(x => x.RequestId);
        await using (var transaction = _context.Database.BeginTransaction())
        {
            var items = requestIds.Select(searchRequestId => new SearchRequestQueueItem
            {
                IsProcessed = false,
                QueuedTimeUtc = _dateTime.UtcNow,
                SearchRequestId = searchRequestId
            });
            if (items.Any())
            {
                await _context.Queue.AddRangeAsync(items);
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }

    //TODO: Get snapshot id from request instead of just returning latest
    public async Task AddOrUpdateResults(SearchRequestState state, IEnumerable<SearchResultDto> results)
    {
        await using (var transaction = _context.Database.BeginTransaction())
        {
            await MarkAsProcessed(state);

            await StoreResults(state, results);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }

    private async Task MarkAsProcessed(SearchRequestState state)
    {
        var requestIds = state.StoredOptions.Values.Select(x => x.RequestId);
        await _context.Queue
            .Include(x => x.SearchRequest)
            .Where(x => requestIds.Contains(x.SearchRequestId))
            .UpdateFromQueryAsync(obj =>
                new SearchRequestQueueItem()
                {
                    IsProcessed = true,
                    QueuedTimeUtc = obj.QueuedTimeUtc,
                    QueueId = obj.QueueId,
                    SearchRequestId = obj.SearchRequestId
                });
    }

    private async Task StoreResults(SearchRequestState state, IEnumerable<SearchResultDto> results)
    {
        var optionToSnapshot = state.StoredOptions.Keys
            .ToDictionary(option => option, option => new SearchRequestSnapshot()
            {
                SearchRequestId = state.StoredOptions[option].RequestId,
                Time = _dateTime.UtcNow
            });
        await _context.AddRangeAsync(optionToSnapshot.Values);

        var optionToItems = results
           .Where(result => result.Items is not null)
           .SelectMany(result => result.Items!.Select(item => (item, option: new SearchRequestOptionDto(result.Website, item.ContentType, SearchRequestTerm.CreateAnd(result.Terms)))))
           .GroupBy(x => x.option)
           .ToDictionary(x => x.Key, x => x.Select(y => y.item).ToList());

        List<SearchResult> resultsToStore = new();
        foreach (var (option, items) in optionToItems)
        {
            if (optionToSnapshot.ContainsKey(option))
            {
                var snapshotId = optionToSnapshot[option].Id;
                foreach (var item in items)
                {
                    resultsToStore.Add(new SearchResult()
                    {
                        FoundTimeUtc = _dateTime.UtcNow,
                        ImagePreviewUrl = item.ImagePreviewUrl,
                        SearchItemUrl = item.SearchItemUrl,
                        AdditionalData = item.Data.ToJObject(),
                        SearchRequestSnapshotId = snapshotId
                    });
                }
            }
            else
            {
                //we are trying to store result that does not correspond with the request that produced it
                //silently ignore for now, we may return something here later
                _logger.LogWarning("Trying to store result that does not correspond to any request option");
            }
        }
        await _context.Result.AddRangeAsync(resultsToStore);

        _logger.LogInformation("Stored '{rowsCount}' rows of new search results", resultsToStore.Count);
    }
}
