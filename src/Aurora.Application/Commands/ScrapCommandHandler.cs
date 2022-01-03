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

        public ScrapCommandHandler(IScraperRunner scraperRunner, ISearchDataService search)
        {
            _scraperRunner = scraperRunner;
            _search = search;
        }

        public async Task<Unit> Handle(ScrapCommand requestWrapper, CancellationToken cancellationToken)
        {
            var request = requestWrapper.SearchRequest;
            var results = await _scraperRunner.Run(request, cancellationToken);
            await _search.AddOrUpdateResults(request, results);
            return Unit.Value;
        }
    }
}
