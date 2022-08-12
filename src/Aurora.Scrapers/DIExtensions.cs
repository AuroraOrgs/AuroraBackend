using Aurora.Application.Scrapers;
using Aurora.Scrapers.Config;
using Aurora.Scrapers.Contracts;
using Aurora.Scrapers.Discovery;
using Aurora.Scrapers.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System.Net;
using System.Reflection;

namespace Aurora.Scrapers
{
    public static class DIExtensions
    {
        public static IServiceCollection AddScrapers(this IServiceCollection services)
        {
            services.AddScoped<DriverInitializer>();
            services.AddHttpClients();
            var (optionScrapers, totalScrapers) = DiscoverScrapers();
            foreach (var scraper in optionScrapers)
            {
                services.AddTransient(scraper);
            }

            foreach (var scraper in totalScrapers)
            {
                services.AddTransient(scraper);
            }

            var scrapersContext = new ScrapersContext(totalScrapers, optionScrapers);
            services.AddSingleton(scrapersContext);
            services.AddSingleton<OptionScraperContext>();
            services.AddScoped<IOptionsScraperCollector, OptionsScraperCollector>();

            return services;
        }

        private static (List<Type> Option, List<Type> Total) DiscoverScrapers()
        {
            var baseScraperType = typeof(IOptionScraper);
            var baseTotalScraperType = typeof(ITotalScraper);
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsAssignableTo(baseScraperType) || x.IsAssignableTo(baseTotalScraperType));
            var optionScrapers = new List<Type>();
            var totalScrapers = new List<Type>();
            foreach (var type in types)
            {
                if (type.IsAssignableTo(baseScraperType))
                {
                    optionScrapers.Add(type);
                }
                else
                {
                    totalScrapers.Add(type);
                }
            }
            return (optionScrapers, totalScrapers);
        }

        private static void AddHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHttpClient(HttpClientNames.DefaultClient, client =>
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.134 YaBrowser/22.7.0.1842 Yowser/2.5 Safari/537.36");
            }).AddWaitAndRetryPolicy();
            //needed for xvideos authentification
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            services.AddHttpClient(HttpClientNames.XVideosClient, client =>
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36 OPR/79.0.4143.72");
            }).AddWaitAndRetryPolicy();
        }

        private static IHttpClientBuilder AddWaitAndRetryPolicy(this IHttpClientBuilder clientBuilder, bool failOnNotFound = false)
        {
            return clientBuilder.AddPolicyHandler((services, policy) =>
            {
                var options = services.GetRequiredService<IOptions<HttpConfig>>().Value;
                var logger = services.GetRequiredService<ILogger<HttpClient>>();
                return Policy.Handle<HttpRequestException>()
                    .Or<Exception>()
                    .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false && (failOnNotFound || r.StatusCode is not HttpStatusCode.NotFound))
                    .WaitAndRetryAsync(options.RetryCount, retryAttempt => TimeSpan.FromMilliseconds(options.WaitFactorMs * retryAttempt), (response, time) =>
                    {
                        if (response is not null)
                        {
                            var exception = response.Exception;
                            if (exception is not null)
                            {
                                logger.LogError(exception, "Receieved an exception whilst making a request - '{msg}'", exception.Message);
                            }

                            var result = response.Result;
                            if (result is not null)
                            {
                                try
                                {
                                    logger.LogInformation("Failed to make a request in  '{time} with '{status}' statusCode and '{reason}' reason", time, (int)result.StatusCode, result.ReasonPhrase);
                                }
                                finally
                                {
                                    result?.Dispose();
                                }
                            }
                        }
                        else
                        {
                            logger.LogInformation("Failed to make a request in '{time}' with no response", time);
                        }
                    });
            });
        }
    }
}
