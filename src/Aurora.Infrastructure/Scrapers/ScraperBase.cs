using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Infrastructure.Models;
using Aurora.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Aurora.Infrastructure.Scrapers
{
    public abstract class ScraperBase : ISearchScraper
    {
        private const string LogFormat = "StatusCode: '{code}', scraper '{scraperName}' {scraperMessage} in {time}";

        private readonly ILogger<ISearchScraper> _logger;

        public ScraperBase(ILogger<ISearchScraper> logger)
        {
            _logger = logger;
        }

        public async Task<ValueOrNull<SearchResult>> Search(SearchRequest request, CancellationToken token = default)
        {
            var watch = Stopwatch.StartNew();
            var searchResult = await PerformSearch(request, token);
            watch.Stop();

            var ranFor = watch.Elapsed;
            LogRun(searchResult, ranFor);

            return searchResult.Result;
        }

        private void LogRun(ExtendedSearchResult searchResult, System.TimeSpan ranFor)
        {
            var time = ranFor.ToString();
            var scraperName = GetType().Name;
            var codeMessage = searchResult.StatusCode.ToString();

            _logger.LogInformation(LogFormat, codeMessage, scraperName, codeMessage, time);
        }

        public async Task<ExtendedSearchResult> PerformSearch(SearchRequest request, CancellationToken token)
        {
            ValueOrNull<SearchResult> result;
            ScraperStatusCode code;
            try
            {
                result = await SearchInner(request, token);
                code = result.HasValue ? ScraperStatusCode.Success : ScraperStatusCode.HandledError;
            }
            catch
            {
                code = ScraperStatusCode.UnhandledError;
                result = ValueOrNull<SearchResult>.CreateNull("Unhandled exception");
            }

            return new ExtendedSearchResult
            {
                Result = result,
                StatusCode = code
            };
        }

        public abstract Task<ValueOrNull<SearchResult>> SearchInner(SearchRequest request, CancellationToken token = default);
    }
}