using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Aurora.Application
{
    public static class DiExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IScraperRunner, ScraperRunner>();
            services.AddMediatR(typeof(DiExtensions).Assembly);
            return services;
        }
    }
}
