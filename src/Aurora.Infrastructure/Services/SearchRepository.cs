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
        var term = SearchRequestTerm.CreateOr(request.SearchTerms);
        var requests = await _context.Request
                        .Include(x => x.QueueItems)
                        .Include(x => x.Snapshots)
                        .Where(x =>
                            request.ContentTypes.Contains(x.ContentType)
                            && request.Websites.Contains(x.Website)
                            && x.SearchTerm == term)
                        .ToListAsync();

        var existingRequestsWithStatus = requests.Select(x => (Request: x, GetStatusFor(x)));
        var existingRequests = existingRequestsWithStatus.Select(x => x.Request);

        var existingOptions = existingRequests.Select(x => new SearchRequestOption(x.Website, x.ContentType, x.SearchTerm));
        List<SearchRequestOption> requestedOptions = new();
        foreach (var website in request.Websites)
        {
            foreach (var option in request.ContentTypes)
            {
                requestedOptions.Add(new(website, option, term));
            }
        }
        var newOptions = requestedOptions.Where(x => existingOptions.NotContains(x));
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
        }).ToArray();

        _context.Request
            .AddRange(newRequests);
        if (isUserGenerated)
        {
            foreach (var existingRequest in existingRequests)
            {
                existingRequest.OccurredCount++;
            }
        }

        _context.Request.UpdateRange(existingRequests);

        var updatedCount = await _context.SaveChangesAsync();
        _logger.LogInformation("Updated '{number}' records whilst fetching", updatedCount);

        IEnumerable<(SearchRequest Request, SearchRequestStatus Status)> allRequests = existingRequestsWithStatus.Union(newRequests.Select(x => (x, SearchRequestStatus.NotFetched)));
        var result = allRequests.ToDictionary(
            key => new SearchRequestOption(key.Request.Website, key.Request.ContentType, key.Request.SearchTerm),
            value => new SearchRequestItem(
                value.Request.Id,
                value.Status,
                value.Request.Snapshots.Select(snapshot => new SearchSnapshot(snapshot.Id, snapshot.Time)).ToList()
              ));
        return new SearchRequestState(result);
    }

    public async Task MarkAsQueued(SearchRequestState request)
    {
        var requestIds = request.StoredRequests.Values.Where(x => x.RequestStatus == SearchRequestStatus.NotFetched).Select(x => x.RequestId);
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
        var requestIds = state.StoredRequests.Values.Select(x => x.RequestId);
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
        var newResults = results
            .Where(result => result.Items is not null)
            .SelectMany(result => result.Items!.Select(item => new SearchResult()
            {
                FoundTimeUtc = _dateTime.UtcNow,
                ImagePreviewUrl = item.ImagePreviewUrl,
                SearchItemUrl = item.SearchItemUrl,
                AdditionalData = item.Data.ToJObject(),
                SearchRequestSnapshotId = GetSnapshotId(state, result, item)
            }))
            .ToList();
        await _context.Result.AddRangeAsync(newResults);

        _logger.LogInformation("Stored '{rowsCount}' rows of new search results", newResults.Count);
    }

    private static Guid GetSnapshotId(SearchRequestState state, SearchResultDto result, SearchItem item) =>
        state.StoredRequests[new(result.Website, item.ContentType, SearchRequestTerm.CreateOr(result.Terms))].Snapshots.OrderBy(x => x.SnapshotTime).Last().SnapshotId;
}
