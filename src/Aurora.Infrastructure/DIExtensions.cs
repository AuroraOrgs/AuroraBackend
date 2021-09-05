using Aurora.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Aurora.Infrastructure
{
    public static class DIExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ISearchScraperCollector, SearchScraperCollector>();

            return services;
        }
    }
}
