using Aurora.Application.Contracts;
using Aurora.Application.Extensions;
using Aurora.Application.Models;
using Aurora.Shared.Extensions;
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
        var websiteStatus = storedRequest.StoredOptions.GroupBy(x => x.Value.OptionStatus)
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
        var resultItems = result.Results;
        foreach (var website in nonCachedWebsites)
        {
            resultItems.Add(new SearchResultDto(request.SearchTerms, website));
        }

        log($"Finished processing in {GetType().Name}");
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
}