using Aurora.Application.Contracts;
using Aurora.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Shared.Extensions;
using Aurora.Application.Extensions;

namespace Aurora.Application.Commands
{
    public class SearchCommandHandler : IRequestHandler<SearchCommand, SearchCommandResult>
    {
        private readonly ISearchDataService _search;
        private readonly IQueueProvider _queue;
        private readonly ILogger<SearchCommandHandler> _logger;

        public SearchCommandHandler(ISearchDataService search, IQueueProvider queue, ILogger<SearchCommandHandler> logger)
        {
            _search = search;
            _queue = queue;
            _logger = logger;
        }

        public async Task<SearchCommandResult> Handle(SearchCommand requestWrapper, CancellationToken cancellationToken)
        {
            var request = requestWrapper.SearchRequest;
            Action<string> log = (prefix) => _logger.LogRequest(request, prefix);
            log("Received request");

            var storedRequest = await _search.FetchRequest(request, true);
            var websiteStatus = storedRequest.StoredRequests.GroupBy(x => x.Value.RequestStatus)
                                                               .ToDictionary(x => x.Key, x => x.Select(y => y.Key.Item1).Distinct());
            var queuedWebsites = websiteStatus.GetOrDefault(SearchRequestStatus.Queued, Enumerable.Empty<SupportedWebsite>());
            var notFetchedWebsites = websiteStatus.GetOrDefault(SearchRequestStatus.NotFetched, Enumerable.Empty<SupportedWebsite>());
            var fetchedWebsites = websiteStatus.GetOrDefault(SearchRequestStatus.Fetched, Enumerable.Empty<SupportedWebsite>());
            log($"Found '{fetchedWebsites.CommaSeparate()}' already processed");
            log($"Found '{notFetchedWebsites.CommaSeparate()}' not fetched");
            log($"Found '{queuedWebsites.CommaSeparate()}' queued");

            SearchResults result;
            if (fetchedWebsites.Any())
            {
                result = await _search.GetResults(storedRequest, requestWrapper.Paging);
            }
            else
            {
                result = new SearchResults(new List<SearchResultDto>(), 0);
            }

            if (notFetchedWebsites.Any())
            {
                await QueueWebsites(requestWrapper.UserId, request.SearchTerm, notFetchedWebsites);
            }

            var nonCachedWebsites = queuedWebsites.Union(notFetchedWebsites);
            var resultItems = result.Results;
            foreach (var website in nonCachedWebsites)
            {
                resultItems.Add(new SearchResultDto(website));
            }

            log($"Finished processing in {GetType().Name}");
            return new SearchCommandResult(resultItems, result.TotalItems);
        }

        private static List<SearchOption> AllOptions = new List<SearchOption>()
                    {
                        SearchOption.Image,
                        SearchOption.Video,
                        SearchOption.Gif
                    };

        private async Task QueueWebsites(string? userId, string searchTerm, IEnumerable<SupportedWebsite> notFetchedWebsites)
        {
            var childRequest = new SearchRequestDto
            {
                SearchTerm = searchTerm,
                SearchOptions = AllOptions,
                Websites = notFetchedWebsites.ToList()
            };

            string websites = String.Join(", ", notFetchedWebsites.Select(x => x.ToString()));
            var newState = await _search.FetchRequest(childRequest, false);
            _queue.Enqueue(
                $"Scrapping {websites} for {searchTerm}",
                new ScrapCommand(childRequest, userId));
            await _search.MarkAsQueued(newState);
        }
    }
}