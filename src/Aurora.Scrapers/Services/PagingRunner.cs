using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Aurora.Scrapers.Services
{
    public class PagingRunner
    {
        private readonly IOptions<ScrapersConfig> _options;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<PagingRunner> _logger;

        public PagingRunner(IOptions<ScrapersConfig> options, IHttpClientFactory clientFactory, ILogger<PagingRunner> logger)
        {
            _options = options;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Runs scraping on paged website
        /// </summary>
        /// <param name="clientName">Name of client to be used for scraping</param>
        /// <param name="loadPage">Function that loads a page based on its number. Numbers start from 0</param>
        /// <param name="scrapPage">Function that finds all of the search results given the document</param>
        /// <param name="findMaxPageNumber">
        /// Optional. Function that determines max possible number of pages for current search options.
        /// Number of pages is inclusive, so if you reply that 5 pages is the max number of pages then you <paramref name="loadPage"/> would get called 5 times.
        /// Default is being provided from configuration
        /// </param>
        /// <param name="pagesWaitTime">Optional. Time to wait in-between scraping pages. Default is 1/4 of a second</param>
        /// <param name="scraperName">Would be set by automatically by the compiler, so please do not set it yourself.</param>
        public async Task RunPagingAsync<T>(string clientName,
            Func<int, HttpClient, Task<ValueOrNull<HtmlDocument>>> loadPage,
            Func<HtmlDocument, Task<List<SearchItem<T>>>> scrapPage,
            Func<HttpClient, Task<ValueOrNull<int>>>? findMaxPageNumber = null,
            TimeSpan? pagesWaitTime = null,
            [CallerMemberName] string scraperName = "")
            where T : SearchResultData
        {
            TimeSpan waitTime = pagesWaitTime ?? TimeSpan.FromMilliseconds(250);
            var options = _options.Value;
            var client = _clientFactory.CreateClient(clientName);
            int maxPageNumber = options.MaxPagesCount;
            if (options.UseLimitations == false)
            {
                if (findMaxPageNumber is null)
                {
                    _logger.LogInformation("Tried to scrap without limitation for '{scraperName}', but no finder were provided", scraperName);
                }
                else
                {
                    maxPageNumber = (await findMaxPageNumber(client))
                        .WithDefault(options.MaxPagesCount, errorMessage => _logger.LogError("Failed to find max page number in '{scraperName}' with message '{msg}'", scraperName, errorMessage));
                }
            }
            List<SearchItem<T>> result = new();
            for (int i = 0; i < maxPageNumber; i++)
            {
                bool failed = false;
                var page = await loadPage(i, client);
                await page.ResolveAsync(async document =>
                {
                    var items = await scrapPage(document);
                    result.AddRange(items);
                }, message =>
                {
                    failed = true;
                    _logger.LogError("Failed to load page for '{scraperName}' with message '{msg}'", scraperName, message);
                    return Task.CompletedTask;
                });

                if (failed || (options.UseLimitations && options.MaxItemsCount <= result.Count))
                {
                    break;
                }
            }
        }
    }
}
