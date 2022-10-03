using Aurora.Application.Scrapers;
using Microsoft.Extensions.DependencyInjection;

namespace Aurora.Application;

public static class DiExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IScraperRunner, ScraperRunner>();
        services.AddScoped<ITotalScraperRunner, TotalScraperRunner>();
        services.AddScoped<IRefreshRunner, RefreshRunner>();
        services.AddMediatR(typeof(DiExtensions).Assembly);
        return services;
    }
}
