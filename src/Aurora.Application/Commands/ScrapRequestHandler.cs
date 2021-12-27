using Aurora.Application.Contracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Application.Commands
{
    public class ScrapRequestHandler : IRequestHandler<ScrapRequest>
    {
        private readonly IScraperRunner _scraperRunner;
        private readonly ISearchDataService _search;

        public ScrapRequestHandler(IScraperRunner scraperRunner, ISearchDataService search)
        {
            _scraperRunner = scraperRunner;
            _search = search;
        }

        public async Task<Unit> Handle(ScrapRequest requestWrapper, CancellationToken cancellationToken)
        {
            var request = requestWrapper.SearchRequest;
            var results = await _scraperRunner.Run(request, cancellationToken);
            await _search.AddOrUpdateResults(request, results);
            return Unit.Value;
        }
    }
}
