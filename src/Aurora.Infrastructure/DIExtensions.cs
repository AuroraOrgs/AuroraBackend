using Aurora.Application.Contracts;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Scrapers;
using Aurora.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aurora.Infrastructure
{
    public static class DIExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<ISearchScraperCollector, SearchScraperCollector>();
            services.AddSingleton<PornhubScraper>();

            services.AddSingleton<IWebClientService, WebClientService>();

            return services;
        }
    }
}
