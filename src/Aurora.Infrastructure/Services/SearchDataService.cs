using Aurora.Application.Contracts;
using Aurora.Application.Entities;
using Aurora.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aurora.Shared.Extensions;

namespace Aurora.Infrastructure.Services
{
    public class SearchDataService : ISearchDataService
    {
        private readonly SearchContext _context;
        private readonly ILogger<SearchDataService> _logger;
        private readonly IDateTimeProvider _dateTime;

        public SearchDataService(SearchContext context, ILogger<SearchDataService> logger, IDateTimeProvider dateTime)
        {
            _context = context;
            _logger = logger;
            _dateTime = dateTime;
        }

        private async Task<IEnumerable<(SearchRequest Request, SearchRequestStatus Status)>> GetExistingFor(SearchRequestDto request)
        {
            //TODO: Include only latest queue item
            var requests = await _context.Request
                            .Include(x => x.QueueItems)
                            .Where(x => request.SearchOptions.Contains(x.ContentOption)
                                && request.Websites.Contains(x.Website)
                                && request.SearchTerm == x.SearchTerm)
                            .ToListAsync();

            return requests.Select(x => (x, GetStatusFor(x)));
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

        private static List<SearchResultDto> Convert(List<SearchResult> results)
        {
            return results.GroupBy(result => result.Request.Website)
                .Select(resultsByWebsite => new SearchResultDto(ConvertResults(resultsByWebsite), resultsByWebsite.Key))
                .ToList();
        }

        private static List<SearchItem> ConvertResults(IEnumerable<SearchResult> results)
        {
            return results
                .Select(result => new SearchItem(result.Request.ContentOption, result.SearchItemUrl, result.ImagePreviewUrl))
                .ToList();
        }

        public async Task<SearchRequestState> FetchRequest(SearchRequestDto request, bool isUserGenerated)
        {
            var existingRequestsWithStatus = await GetExistingFor(request);
            var existingRequests = existingRequestsWithStatus.Select(x => x.Request);

            var existingOptions = existingRequests.Select(x => (x.Website, x.ContentOption));
            List<(SupportedWebsite website, SearchOption option)> requestedOptions = new List<(SupportedWebsite, SearchOption)>();
            foreach (var website in request.Websites)
            {
                foreach (var option in request.SearchOptions)
                {
                    requestedOptions.Add((website, option));
                }
            }
            var newOptions = requestedOptions.Where(x => existingOptions.NotContains(x));

            var newRequests = newOptions.Select(newOption =>
            {
                var newRequest = new SearchRequest()
                {
                    ContentOption = newOption.option,
                    OccurredCount = 1,
                    SearchTerm = request.SearchTerm,
                    Website = newOption.website
                };
                _logger.LogInformation(
                    "Creating request with '{0}' option, '{1}' website and '{2} term'",
                    newOption.option, newOption.website, request.SearchTerm);
                return newRequest;
            });

            await _context.Request
                .AddRangeAsync(newRequests);
            if (isUserGenerated)
            {
                foreach (var existingRequest in existingRequests)
                {
                    existingRequest.OccurredCount++;
                }
            }
           
            _context.Request.UpdateRange(existingRequests);

            await _context.SaveChangesAsync();

            var allRequests = existingRequestsWithStatus.Union(newRequests.Select(x => (x, SearchRequestStatus.NotFetched)));
            var result = allRequests.ToDictionary(key => (key.Item1.Website, key.Item1.ContentOption), value => (value.Item1.Id, value.Item2));
            return new SearchRequestState(result);
        }

        public async Task<SearchResults> GetResults(SearchRequestState state, PagingOptions paging)
        {
            var idsToLoad = state.StoredRequests.Values.Where(x => x.RequestStatus == SearchRequestStatus.Fetched).Select(x => x.RequestId);
            SearchResults result;
            if (idsToLoad.Any())
            {
                var filteredResults = _context.Result.Include(x => x.Request)
                                                     .Where(x => idsToLoad.Contains(x.RequestId));
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

                var results = Convert(storedResults);
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

        public async Task MarkAsQueued(SearchRequestState request)
        {
            var itemsToQueue = request.StoredRequests.Values.Where(x => x.RequestStatus == SearchRequestStatus.NotFetched).Select(x => x.RequestId);
            var items = itemsToQueue.Select(x => new SearchRequestQueueItem
            {
                IsProcessed = false,
                QueuedTimeUtc = _dateTime.UtcNow,
                SearchRequestId = x
            });
            await _context.Queue.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }

        public async Task AddOrUpdateResults(SearchRequestState state, IEnumerable<SearchResultDto> results)
        {
            var existingIds = state.StoredRequests.Values.Select(x => x.RequestId);

            _logger.LogInformation("Removing stale search results");
            var removedRows = await _context.Result
                .Where(x => existingIds.Contains(x.RequestId))
                .DeleteFromQueryAsync();
            _logger.LogInformation("Removed '{0}' rows of stale search results", removedRows);

            List<SearchResult> finalResults = results.Select(
                result => result.Items.Select(item => new SearchResult()
                {
                    FoundTimeUtc = _dateTime.UtcNow,
                    ImagePreviewUrl = item.ImagePreviewUrl,
                    RequestId = state.StoredRequests[(result.Website, item.Option)].RequestId,
                    SearchItemUrl = item.SearchItemUrl
                }
           )).Flatten().ToList();

            await _context.Queue.Where(x => existingIds.Contains(x.SearchRequestId)).UpdateFromQueryAsync(obj =>
            new SearchRequestQueueItem()
            {
                IsProcessed = true,
                QueuedTimeUtc = obj.QueuedTimeUtc,
                QueueId = obj.QueueId,
                SearchRequestId = obj.SearchRequestId
            });

            await _context.Result
                .AddRangeAsync(finalResults);
            _logger.LogInformation("Stored '{0}' rows of new search results", finalResults.Count);

            await _context.SaveChangesAsync();
        }
    }
}
