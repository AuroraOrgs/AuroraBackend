using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Application.Contracts;
using Aurora.Application.Enums;
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

        public async Task<ValueOrNull<SearchResultDto>> Search(SearchRequestDto request, CancellationToken token = default)
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

        private async Task<SearchResultDto> GetResult(SearchRequestDto request, CancellationToken token)
        {
            List<SearchItem> items = new ();
            if (request.SearchOptions.Contains(SearchOption.Video))
            {
                var response = await SearchVideosInner(request, token);
                Resolve(items, response);
            }

            if (request.SearchOptions.Contains(SearchOption.Gif))
            {
                var response = await SearchGifsInner(request, token);
                Resolve(items, response);
            }

            if (request.SearchOptions.Contains(SearchOption.Image))
            {
                var response = await SearchImagesInner(request, token);
                Resolve(items, response);
            }

            return new(items, WebSite);
        }

        private static void Resolve(List<SearchItem> items, ValueOrNull<List<SearchItem>> response)
        {
            response.Resolve(responseItems =>
            {
                items.AddRange(responseItems);
            });
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