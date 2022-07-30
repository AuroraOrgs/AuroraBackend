﻿using Aurora.Application.Contracts;
using Aurora.Infrastructure.Bridge;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Extensions;
using Aurora.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
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
            services.AddScoped<IOptionsScraperCollector, OptionsScraperCollector>();
            services.AddScoped<IWebClientService, WebClientService>();
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
    }
}
