using Aurora.Application.Contracts;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Application.Commands
{
    public class ScrapCommandHandler : IRequestHandler<ScrapCommand>
    {
        private readonly IScraperRunner _scraperRunner;
        private readonly ISearchDataService _search;
        private readonly INotificator _notificator;

        public ScrapCommandHandler(IScraperRunner scraperRunner, ISearchDataService search, INotificator notificator)
        {
            _scraperRunner = scraperRunner;
            _search = search;
            _notificator = notificator;
        }

        public async Task<Unit> Handle(ScrapCommand requestWrapper, CancellationToken cancellationToken)
        {
            var request = requestWrapper.SearchRequest;
            var userId = requestWrapper.UserId;
            if (userId is not null)
            {
                _scraperRunner.RequestProcessed += (_, req) =>
                {
                    _notificator.NotifyAboutScrapFinishing(userId, req);
                };
            }
            var results = await _scraperRunner.Run(request, cancellationToken);
            await _search.AddOrUpdateResults(request, results);
            return Unit.Value;
        }
    }
}
