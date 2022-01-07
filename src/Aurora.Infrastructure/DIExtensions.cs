using Aurora.Application.Contracts;
using Aurora.Infrastructure.Bridge;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Scrapers;
using Aurora.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aurora.Infrastructure
{
    public static class DIExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<ISearchScraperCollector, SearchScraperCollector>();
            services.AddScoped<PornhubScraper>();
            services.AddScoped<XvideosScraper>();
            services.AddScoped<IWebClientService, WebClientService>();
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

            services.AddDbContext<SearchContext>(x =>
            {
                x.UseSqlServer(mainConnectionString, b =>
                {
                    b.MigrationsAssembly("Aurora.Infrastructure");
                });
            });

            return services;
        }
    }
}
