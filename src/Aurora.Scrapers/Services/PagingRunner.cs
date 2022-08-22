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

        public async Task RunPagingAsync<T>(string clientName,
            Func<int, HttpClient, Task<ValueOrNull<HtmlDocument>>> loadPage,
            Func<HtmlDocument, Task<List<SearchItem<T>>>> scrapPage,
            Func<HttpClient, Task<ValueOrNull<int>>>? findMaxPageNumber = null,
            [CallerMemberName] string scraperName = "")
            where T : SearchResultData
        {
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
