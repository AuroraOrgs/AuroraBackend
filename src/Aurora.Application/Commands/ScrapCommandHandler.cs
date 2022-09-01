using Aurora.Application.Contracts;
using Aurora.Application.Extensions;
using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Aurora.Application.Commands;

public class ScrapCommandHandler : IRequestHandler<ScrapCommand>
{
    private readonly IScraperRunner _scraperRunner;
    private readonly ISearchRepository _repo;
    private readonly INotificator _notificator;
    private readonly ILogger<ScrapCommandHandler> _logger;

    public ScrapCommandHandler(
        IScraperRunner scraperRunner,
        ISearchRepository repo,
        INotificator notificator,
        ILogger<ScrapCommandHandler> logger)
    {
        _scraperRunner = scraperRunner;
        _repo = repo;
        _notificator = notificator;
        _logger = logger;
    }

    public async Task<Unit> Handle(ScrapCommand requestWrapper, CancellationToken cancellationToken)
    {
        var request = requestWrapper.SearchRequest;
        var userId = requestWrapper.UserId;
        var results = await _scraperRunner.RunAsync(request, result =>
        {
            Task task;
            if (userId is not null)
            {
                _logger.LogRequest(requestWrapper.SearchRequest, $"Notifying {userId}");
                task = _notificator.NotifyAboutScrapFinishing(userId, result);
            }
            else
            {
                task = Task.CompletedTask;
            }
            return task;
        }, cancellationToken);
        _logger.LogRequest(request, $"Got '{results.Count}' results with '{GetProcessedCount(results)}' total count of items");
        var requestStored = await _repo.FetchRequest(request, false);
        await _repo.AddOrUpdateResults(requestStored, results);
        return Unit.Value;
    }

    private static long GetProcessedCount(List<SearchResultDto> results)
    {
        return results.Where(x => x.BeenQueued == false)
                      .SelectMany(x => x.Items!)
                      .LongCount();
    }
}
