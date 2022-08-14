using Aurora.Application.Contracts;
using Aurora.Infrastructure.Bridge;
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
            services.AddScoped<ISearchDataService, SearchDataService>();
            services.AddScoped<IQueueProvider, QueueProvider>();
            services.AddScoped<IDateTimeProvider, SystemClockDateTimeProvider>();
            services.AddDistributedMemoryCache();
            services.AddSignalR();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            string mainConnectionString = config.GetConnectionString("MainDb");

            services.AddTransient<INotificator, Notificator>();

            services.AddHangfireServer()
                .AddHangfire(mainConnectionString);

            services.AddDbContext<SearchContext>(x =>
            {
                x.UseNpgsql(mainConnectionString, b =>
                {
                    b.MigrationsAssembly("Aurora.Infrastructure");
                });
            });

            return services;
        }

        private static IServiceCollection AddHangfire(this IServiceCollection services, string mainConnectionString)
        {
            var sqlOptions = new PostgreSqlStorageOptions()
            {
                PrepareSchemaIfNecessary = true,
                SchemaName = "hangfire"
            };
            services.AddHangfire(configuration =>
            {
                configuration
                .UsePostgreSqlStorage(mainConnectionString, sqlOptions)
                .UseMediatR();
            });
            JobStorage.Current = new PostgreSqlStorage(mainConnectionString, sqlOptions);

            return services;
        }
    }
}
