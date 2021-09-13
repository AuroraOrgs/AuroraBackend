using Aurora.Application.Contracts;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Scrapers;
using Aurora.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aurora.Infrastructure
{
    public static class DIExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<ISearchScraperCollector, SearchScraperCollector>();
            services.AddSingleton<PornhubScraper>();

            services.AddSingleton<IWebClientService, WebClientService>();

            services.Configure<SeleniumConfig>(option => config.GetSection("Selenium").Bind(option));

            return services;
        }
    }
}
