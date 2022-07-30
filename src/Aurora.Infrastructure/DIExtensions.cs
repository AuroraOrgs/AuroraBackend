using Aurora.Application.Contracts;
using Aurora.Infrastructure.Bridge;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Extensions;
using Aurora.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Net;
using System.Net.Http;

namespace Aurora.Infrastructure
{
    public static class DIExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IOptionsScraperCollector, OptionsScraperCollector>();
            services.AddScoped<DriverInitializer>();
            services.AddScoped<ISearchDataService, SearchDataService>();
            services.AddScoped<IQueueProvider, QueueProvider>();
            services.AddScoped<IDateTimeProvider, SystemClockDateTimeProvider>();
            services.AddDistributedMemoryCache();
            services.BindConfigSections(config);
            services.AddSignalR();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            string mainConnectionString = config.GetConnectionString("MainDb");
            services.AddHangfire(configuration =>
            {
                configuration
                .UsePostgreSqlStorage(mainConnectionString)
                .UseMediatR();
            });

            services.AddHttpClients();

            services.AddTransient<INotificator, Notificator>();

            services.AddHangfireServer();

            services.AddDbContext<SearchContext>(x =>
            {
                x.UseNpgsql(mainConnectionString, b =>
                {
                    b.MigrationsAssembly("Aurora.Infrastructure");
                });
            });

            return services;
        }

        private static void AddHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHttpClient(HttpClientNames.PornhubClient, client =>
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;");
            }).AddWaitAndRetryPolicy();
            //needed for xvideos authentification
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            services.AddHttpClient(HttpClientNames.XVideosClient, client =>
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36 OPR/79.0.4143.72");
            }).AddWaitAndRetryPolicy();
        }

        private static IHttpClientBuilder AddWaitAndRetryPolicy(this IHttpClientBuilder clientBuilder)
        {
            return clientBuilder.AddPolicyHandler((services, policy) =>
            {
                var options = services.GetRequiredService<IOptions<HttpConfig>>().Value;
                var logger = services.GetRequiredService<ILogger<HttpClient>>();
                return Policy.Handle<HttpRequestException>()
                    .Or<Exception>()
                    .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
                    .WaitAndRetryAsync(options.RetryCount, retryAttempt => TimeSpan.FromSeconds(options.WaitFactorMs * retryAttempt), (response, time) =>
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
                                    logger.LogInformation("Failed to make a request in  '{time} with '{status}' statusCode and '{reason}' reason", time, result.StatusCode, result.ReasonPhrase);
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
