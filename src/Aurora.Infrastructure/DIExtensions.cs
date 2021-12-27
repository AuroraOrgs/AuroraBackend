using Aurora.Application.Contracts;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Scrapers;
using Aurora.Infrastructure.Services;
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
            services.Configure<SeleniumConfig>(option => config.GetSection("Selenium").Bind(option));

            services.AddDbContext<SearchContext>(x =>
            {
                x.UseNpgsql(config.GetConnectionString("MainDb"), b =>
                {
                    b.MigrationsAssembly("Aurora.Infrastructure");
                });
            });

            return services;
        }
    }
}
