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

        private readonly ILogger _logger;

        public ScraperBase(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<ValueOrNull<SearchResult>> Search(SearchRequest request, CancellationToken token = default)
        {
            Stopwatch watch = new();
            watch.Start();

            ValueOrNull<SearchResult> result;
            ScraperStatusCode code;
            try
            {
                result = await SearchInner(request, token);
                if (result.HasValue)
                {
                    code = ScraperStatusCode.Success;
                }
                else
                {
                    code = ScraperStatusCode.HandledError;
                }
            }
            catch
            {
                code = ScraperStatusCode.UnhandledError;
                result = ValueOrNull<SearchResult>.CreateNull("Unahndled exception");
            }

            watch.Stop();

            string time = watch.ElapsedMilliseconds.ToString();
            string scraperName = GetType().Name;
            string codeMessage = code.ToString();

            _logger.LogDebug(LogFormat, codeMessage, scraperName, codeMessage, time);

            return result;
        }

        public abstract Task<ValueOrNull<SearchResult>> SearchInner(SearchRequest request, CancellationToken token = default);
    }
}