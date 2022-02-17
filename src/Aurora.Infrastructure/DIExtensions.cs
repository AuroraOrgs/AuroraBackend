using System;
using Aurora.Application;
using Aurora.Application.Contracts;
using Aurora.Infrastructure.Bridge;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Scrapers;
using Aurora.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Aurora.Infrastructure
{
    public static class DIExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<ISearchScraperCollector, SearchScraperCollector>();
            services.AddScoped<PornhubScraper>();
            services.AddScoped<XvideosScraper>();
            services.AddScoped<DriverInitializer>();
            services.AddScoped<ISearchDataService, SearchDataService>();
            services.AddScoped<IQueueProvider, QueueProvider>();
            services.AddDistributedMemoryCache();
            services.Configure<SeleniumConfig>(option => config.GetSection("Selenium").Bind(option));
            services.AddSignalR();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            string mainConnectionString = config.GetConnectionString("MainDb");
            services.AddHangfire(configuration =>
            {
                configuration
                .UseSqlServerStorage(mainConnectionString)
                .UseMediatR();
            });

            services.AddTransient<INotificator, Notificator>();

            services.AddHangfireServer();

            services.AddDefaultHttpClient(HttpClientConstants.PornhubClient, "https://www.pornhub.com");
            services.AddDefaultHttpClient(HttpClientConstants.XvideosClient, "https://www.xvideos.com");
            services.AddDefaultHttpClient(HttpClientConstants.RTPornhubClient, "https://rt.pornhub.com");

            services.AddDbContext<SearchContext>(x =>
            {
                x.UseSqlServer(mainConnectionString, b =>
                {
                    b.MigrationsAssembly("Aurora.Infrastructure");
                });
            });

            return services;
        }

        private static IServiceCollection AddDefaultHttpClient(this IServiceCollection services, string name, string url)
        {
            services.AddHttpClient(name, client =>
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "AuroraApplication");
            }).AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600)));
                
            return services;
        }
    }
}
