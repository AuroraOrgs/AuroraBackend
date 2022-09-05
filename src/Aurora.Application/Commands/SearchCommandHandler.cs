using Aurora.Application.Contracts;
using Aurora.Application.Extensions;
using Aurora.Application.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Aurora.Application.Commands;

public class SearchCommandHandler : IRequestHandler<SearchCommand, SearchCommandResult>
{
    private readonly ISearchRepository _repo;
    private readonly IQueueProvider _queue;
    private readonly ISearchQueryService _query;
    private readonly ILogger<SearchCommandHandler> _logger;

    public SearchCommandHandler(ISearchRepository search, IQueueProvider queue, ISearchQueryService query, ILogger<SearchCommandHandler> logger)
    {
        _repo = search;
        _queue = queue;
        _query = query;
        _logger = logger;
    }

    public async Task<SearchCommandResult> Handle(SearchCommand requestWrapper, CancellationToken cancellationToken)
    {
        var request = requestWrapper.SearchRequest;
        Action<string> log = (prefix) => _logger.LogRequest(request, prefix);
        log("Received request");

        var storedRequest = await _repo.FetchRequest(request, true);
        var websiteStatus = storedRequest.StoredOptions.GroupBy(x => GetStatusFor(x.Value.Snapshots))
                                                           .ToDictionary(x => x.Key, x => x.Select(y => y.Key.Website).Distinct());
        var queuedWebsites = websiteStatus.GetOrDefault(SearchRequestOptionStatus.Queued, Enumerable.Empty<SupportedWebsite>());
        var notFetchedWebsites = websiteStatus.GetOrDefault(SearchRequestOptionStatus.NotFetched, Enumerable.Empty<SupportedWebsite>());
        var fetchedWebsites = websiteStatus.GetOrDefault(SearchRequestOptionStatus.Fetched, Enumerable.Empty<SupportedWebsite>());
        log($"Found '{fetchedWebsites.CommaSeparate()}' already processed");
        log($"Found '{notFetchedWebsites.CommaSeparate()}' not fetched");
        log($"Found '{queuedWebsites.CommaSeparate()}' queued");

        SearchResults result;
        if (fetchedWebsites.Any())
        {
            result = await _query.GetResults(storedRequest, requestWrapper.Paging);
        }
        else
        {
            result = new SearchResults(new List<SearchResultDto>(), 0);
        }

        if (notFetchedWebsites.Any())
        {
            await QueueWebsites(requestWrapper.UserId, request.SearchTerms, notFetchedWebsites);
        }

        var nonCachedWebsites = queuedWebsites.Union(notFetchedWebsites);
        var resultItems = result.Results.Select(x => x.ToOneOf<SearchResultDto, QueuedResult>())
            .Union(nonCachedWebsites.Select(website => new QueuedResult(true, website).ToOneOf<SearchResultDto, QueuedResult>()))
            .ToList();

        var resultsCount = resultItems.SelectFirsts().Sum(x => x.Items.Count);
        log($"Finished processing in {GetType().Name}, got '{resultsCount}' result items");
        return new SearchCommandResult(resultItems, result.TotalItems);
    }

    private async Task QueueWebsites(string? userId, List<string> searchTerms, IEnumerable<SupportedWebsite> notFetchedWebsites)
    {
        var childRequest = new SearchRequestDto
        {
            SearchTerms = searchTerms,
            ContentTypes = ContentTypeContext.ContentTypes,
            Websites = notFetchedWebsites.ToList()
        };

        string websites = String.Join(", ", notFetchedWebsites.Select(x => x.ToString()));
        var newState = await _repo.FetchRequest(childRequest, false);
        _queue.Enqueue(
            $"Scrapping {websites} for {searchTerms.CommaSeparate()}",
            new ScrapCommand(childRequest, userId));
        await _repo.MarkAsQueued(newState);
    }


    private static SearchRequestOptionStatus GetStatusFor(IEnumerable<SearchSnapshot> snapshots)
    {
        SearchRequestOptionStatus status;
        if (snapshots is null || snapshots.None())
        {
            status = SearchRequestOptionStatus.NotFetched;
        }
        else
        {
            //we might want to use time of queue items to create more informed statuses for snapshots
            if (snapshots.Any(x => x.IsProcessed))
            {
                status = SearchRequestOptionStatus.Fetched;
            }
            else
            {
                status = SearchRequestOptionStatus.Queued;
            }
        }
        return status;
    }

}