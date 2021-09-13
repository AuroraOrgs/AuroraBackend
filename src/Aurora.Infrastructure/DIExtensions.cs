using Aurora.Application.Contracts;
using Aurora.Infrastructure.Scrapers;
using Microsoft.Extensions.DependencyInjection;

namespace Aurora.Infrastructure
{
    public static class DIExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<ISearchScraperCollector, SearchScraperCollector>();
            services.AddSingleton<PornhubScraper>();

            return services;
        }
    }
}
