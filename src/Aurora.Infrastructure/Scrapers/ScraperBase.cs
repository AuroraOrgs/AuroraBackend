using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        protected readonly ILogger<ISearchScraper> _logger;

        public ScraperBase(ILogger<ISearchScraper> logger)
        {
            _logger = logger;
        }

        public async Task<ValueOrNull<SearchResultDto>> Search(SearchRequestDto request, CancellationToken token = default)
        {
            var watch = Stopwatch.StartNew();
            var searchResult = await PerformSearch(request, token);
            watch.Stop();

            var ranFor = watch.Elapsed;
            LogRun(searchResult, ranFor);

            return searchResult.Result;
        }

        private void LogRun(ExtendedSearchResult searchResult, TimeSpan ranFor)
        {
            var time = ranFor.ToString();
            var scraperName = GetType().Name;
            var codeMessage = searchResult.StatusCode.ToString();

            _logger.LogInformation(LogFormat, codeMessage, scraperName, codeMessage, time);
        }

        public async Task<ExtendedSearchResult> PerformSearch(SearchRequestDto request, CancellationToken token)
        {
            ValueOrNull<SearchResultDto> resultOrNull;

            ScraperStatusCode code;
            try
            {
                resultOrNull = await GetResult(request, token);
                code = ScraperStatusCode.Success;
            }
            catch (Exception exception)
            {
                resultOrNull = ValueOrNull<SearchResultDto>.CreateNull(exception.Message);
                code = ScraperStatusCode.UnhandledError;
            }

            return new ExtendedSearchResult
            {
                Result = resultOrNull,
                StatusCode = code
            };
        }

        //Used to prevent excessive allocation
        private static readonly List<SearchItem> _emptyList = new();

        private async Task<SearchResultDto> GetResult(SearchRequestDto request, CancellationToken token)
        {
            var scrapingTasks = request.SearchOptions.Select(
                option => option switch
                {
                    SearchOption.Video => ExecuteScraping(request, token, (request, token) => SearchVideosInner(request, token)),
                    SearchOption.Image => ExecuteScraping(request, token, (request, token) => SearchGifsInner(request, token)),
                    SearchOption.Gif => ExecuteScraping(request, token, (request, token) => SearchImagesInner(request, token)),
                    _ => throw new Exception($"Non-exhaustive switch case for search option {option}")
                });
            var results = await Task.WhenAll(scrapingTasks);
            var items = results
                .Select(x => x.WithDefault(_emptyList))
                .Aggregate(new List<SearchItem>(), (currentItems, result) =>
                {
                    currentItems.AddRange(result);
                    return currentItems;
                });

            return new(items, WebSite);
        }

        private async Task<ValueOrNull<List<SearchItem>>> ExecuteScraping(
            SearchRequestDto request,
            CancellationToken token,
            Func<SearchRequestDto, CancellationToken, Task<ValueOrNull<List<SearchItem>>>> scrappingFunc
            )
        {
            ValueOrNull<List<SearchItem>> result;
            try
            {
                result = await scrappingFunc(request, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to scrap with following message '{0}'", ex.Message);
                result = ValueOrNull<List<SearchItem>>.CreateNull($"Failed to scrap with {ex.Message}");
            }
            return result;
        }

        //TODO: Move those into separate child, because some website may have mixed items
        public virtual Task<ValueOrNull<List<SearchItem>>> SearchVideosInner(SearchRequestDto request, CancellationToken token = default)
        {
            return Task.FromResult(ValueOrNull<List<SearchItem>>.CreateNull($"Videos are not supported by {GetType().Name}"));
        }

        public virtual Task<ValueOrNull<List<SearchItem>>> SearchImagesInner(SearchRequestDto request, CancellationToken token = default)
        {
            return Task.FromResult(ValueOrNull<List<SearchItem>>.CreateNull($"Images are not supported by {GetType().Name}"));
        }

        public virtual Task<ValueOrNull<List<SearchItem>>> SearchGifsInner(SearchRequestDto request, CancellationToken token = default)
        {
            return Task.FromResult(ValueOrNull<List<SearchItem>>.CreateNull($"Gifs are not supported by {GetType().Name}"));
        }

        public abstract SupportedWebsite WebSite { get; }

        protected static string FormatTermToUrl(string term)
        {
            return term.Replace(" ", "+");
        }
    }
}