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
            ValueOrNull<SearchResult> result = new();

            ScraperStatusCode code;
            try
            {
                if (request.SearchOptions.Contains(SearchOption.Video))
                {
                    var response = await SearchVideosInner(request, token);
                    result.Value.Items.AddRange(response.Value?.Items);
                }

                if (request.SearchOptions.Contains(SearchOption.Gif))
                {
                    var response = await SearchGifsInner(request, token);
                    result.Value.Items.AddRange(response.Value?.Items);
                }

                if (request.SearchOptions.Contains(SearchOption.Image))
                {
                    var response = await SearchImagesInner(request, token);
                    result.Value.Items.AddRange(response.Value?.Items);
                }

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

        public virtual Task<ValueOrNull<SearchResult>> SearchVideosInner(SearchRequest request, CancellationToken token = default)
        {
            return default;
        }

        public virtual Task<ValueOrNull<SearchResult>> SearchImagesInner(SearchRequest request, CancellationToken token = default)
        {
            return default;
        }

        public virtual Task<ValueOrNull<SearchResult>> SearchGifsInner(SearchRequest request, CancellationToken token = default)
        {
            return default;
        }

        protected static string FormatTermToUrl(string term)
        {
            return term.Replace(" ", "+");
        }
    }
}