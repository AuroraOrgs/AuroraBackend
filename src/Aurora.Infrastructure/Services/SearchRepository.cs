﻿using Aurora.Application.Contracts;
using Aurora.Application.Entities;
using Aurora.Application.Models;
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

    private SearchRequestStatus GetStatusFor(SearchRequest request)
    {
        SearchRequestStatus status;
        if (request.QueueItems.None())
        {
            status = SearchRequestStatus.NotFetched;
        }
        else
        {
            var orderedItems = request.QueueItems.OrderByDescending(x => x.QueuedTimeUtc);
            if (orderedItems.Count() > 1)
            {
                if (orderedItems.Skip(1).First().IsProcessed)
                {
                    status = SearchRequestStatus.Fetched;
                }
                else
                {
                    //Should not be possible, but we better be safe
                    status = SearchRequestStatus.Queued;
                    _logger.LogInformation("Found two non-processed queue items for {requestId}", request.Id);
                }
            }
            else
            {
                var latestQueueItem = orderedItems.First();
                if (latestQueueItem.IsProcessed)
                {
                    status = SearchRequestStatus.Fetched;
                }
                else
                {
                    status = SearchRequestStatus.Queued;
                }
            }
        }
        return status;
    }

    public async Task<SearchRequestState> FetchRequest(SearchRequestDto request, bool isUserGenerated)
    {
        var requests = await _context.Request
                        .Include(x => x.QueueItems)
                        .Where(x => request.ContentTypes.Contains(x.ContentType)
                            && request.Websites.Contains(x.Website)
                            && request.SearchTerms.Contains(x.SearchTerm))
                        .ToListAsync();

        var existingRequestsWithStatus = requests.Select(x => (Request: x, GetStatusFor(x)));
        var existingRequests = existingRequestsWithStatus.Select(x => x.Request);

        var existingOptions = existingRequests.Select(x => (x.Website, x.ContentType, x.SearchTerm));
        List<(SupportedWebsite website, ContentType type, string term)> requestedOptions = new List<(SupportedWebsite, ContentType, string)>();
        foreach (var website in request.Websites)
        {
            foreach (var option in request.ContentTypes)
            {
                foreach (var term in request.SearchTerms)
                {
                    requestedOptions.Add((website, option, term));
                }
            }
        }
        var newOptions = requestedOptions.Where(x => existingOptions.NotContains(x));
        _logger.LogInformation(
              "Creating new requests with '{0}' contentTypes, '{1}' websites and '{2} terms'",
              newOptions.Select(x => x.type).CommaSeparate(),
              newOptions.Select(x => x.website).CommaSeparate(),
              newOptions.Select(x => x.term).CommaSeparate());
        var newRequests = newOptions.Select(newOption => new SearchRequest()
        {
            ContentType = newOption.type,
            OccurredCount = 1,
            SearchTerm = newOption.term,
            Website = newOption.website
        })
            .ToArray();

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

        var allRequests = existingRequestsWithStatus.Union(newRequests.Select(x => (x, SearchRequestStatus.NotFetched)));
        var result = allRequests.ToDictionary(key => (key.Item1.Website, key.Item1.ContentType, key.Item1.SearchTerm), value => (value.Item1.Id, value.Item2));
        return new SearchRequestState(result);
    }

    public async Task MarkAsQueued(SearchRequestState request)
    {
        var searchRequestIds = request.StoredRequests.Values.Where(x => x.RequestStatus == SearchRequestStatus.NotFetched).Select(x => x.RequestId);
        await using (var transaction = _context.Database.BeginTransaction())
        {
            var removedCount = await _context.Queue.Where(x => searchRequestIds.Contains(x.SearchRequestId)).DeleteFromQueryAsync();
            _logger.LogInformation("Removed '{itemsCount}' items due to requeue", removedCount);
            var items = searchRequestIds.Select(searchRequestId => new SearchRequestQueueItem
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

    public async Task AddOrUpdateResults(SearchRequestState state, IEnumerable<SearchResultDto> results)
    {
        var existingRequests = state.StoredRequests.Values.Select(x => x.RequestId);
        await using (var transaction = _context.Database.BeginTransaction())
        {
            await ClearOldResults(results, existingRequests);

            await MarkAsProcessed(existingRequests);

            await StoreResults(state, results);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }

    private async Task ClearOldResults(IEnumerable<SearchResultDto> results, IEnumerable<Guid> existingIds)
    {
        _logger.LogInformation("Removing stale search results");
        var removedRows = await _context.Result
            .Where(x => x.Requests.Count() == 1 && x.Requests.Any(request => existingIds.Contains(request.SearchRequestId)))
            .DeleteFromQueryAsync();
        _logger.LogInformation("Removed '{0}' stale results with no other requests", removedRows);
        var websitedToRecreate = results.Select(x => x.Website);
        var typesToRecreate = results.Select(x => x.Items!.Select(y => y.ContentType)).Flatten().Distinct();
        var removedRelations = await _context.RequestToResult
            .Include(x => x.SearchRequest)
            .Where(x => existingIds.Contains(x.SearchResultId) && websitedToRecreate.Contains(x.SearchRequest.Website) && typesToRecreate.Contains(x.SearchRequest.ContentType))
            .DeleteFromQueryAsync();
        _logger.LogInformation("Removed '{0}' relations between result and request", removedRelations);
    }

    private async Task MarkAsProcessed(IEnumerable<Guid> existingIds)
    {
        await _context.Queue
            .Where(x => existingIds.Contains(x.SearchRequestId))
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
        var resultToIds = results.Select(result => result.Items!.Select(item =>
                               (
                                    result.Terms.Select(term => state.StoredRequests[(result.Website, item.ContentType, term)].RequestId),
                                    new SearchResult()
                                    {
                                        FoundTimeUtc = _dateTime.UtcNow,
                                        ImagePreviewUrl = item.ImagePreviewUrl,
                                        SearchItemUrl = item.SearchItemUrl,
                                        AdditionalData = item.Data.ToJObject()
                                    })))
                           .Flatten()
                           .ToDictionary(x => x.Item2, x => x.Item1);

        var newResults = resultToIds.Keys.ToList();
        await _context.Result.AddRangeAsync(newResults);
        var relations = newResults
            .SelectMany(result =>
                resultToIds[result].Select(requestId => new SearchRequestToResult()
                {
                    SearchRequestId = requestId,
                    SearchResultId = result.Id
                }))
            .ToList();

        await _context.RequestToResult.AddRangeAsync(relations);

        _logger.LogInformation("Stored '{0}' rows of new search results and created '{1}' relations", newResults.Count, relations.Count);
    }
}