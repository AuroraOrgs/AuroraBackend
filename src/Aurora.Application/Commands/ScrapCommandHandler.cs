using Aurora.Application.Contracts;
using Aurora.Application.Extensions;
using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Application.Commands
{
    public class ScrapCommandHandler : IRequestHandler<ScrapCommand>
    {
        private readonly IScraperRunner _scraperRunner;
        private readonly ISearchDataService _search;
        private readonly INotificator _notificator;
        private readonly ILogger<ScrapCommandHandler> _logger;

        public ScrapCommandHandler(
            IScraperRunner scraperRunner,
            ISearchDataService search,
            INotificator notificator,
            ILogger<ScrapCommandHandler> logger)
        {
            _scraperRunner = scraperRunner;
            _search = search;
            _notificator = notificator;
            _logger = logger;
        }

        public async Task<Unit> Handle(ScrapCommand requestWrapper, CancellationToken cancellationToken)
        {
            var request = requestWrapper.SearchRequest;
            var userId = requestWrapper.UserId;
            if (userId is not null)
            {
                _logger.LogRequest(requestWrapper.SearchRequest, $"Notifying {userId}");
                _scraperRunner.RequestProcessed += (_, req) =>
                {
                    _notificator.NotifyAboutScrapFinishing(userId, req);
                };
            }
            var results = await _scraperRunner.Run(request, cancellationToken);
            _logger.LogRequest(request, $"Got '{results.Count}' results with '{GetProcessedCount(results)}' total count of items");
            var requestStored = await _search.FetchRequest(request, false);
            await _search.AddOrUpdateResults(requestStored, results);
            return Unit.Value;
        }

        private static long GetProcessedCount(List<SearchResultDto> results)
        {
            return results.Where(x => x.BeenQueued == false)
                          .SelectMany(x => x.Items!)
                          .LongCount();
        }
    }
}
