using Aurora.Application;
using Aurora.Infrastructure;
using Aurora.Infrastructure.Services;
using Aurora.Presentation.Services;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Aurora.Presentation
{
    public class Startup
    {
        private static readonly string CorsPolicyName = "AllowAll";
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddApplication()
                .AddInfrastructure(Configuration);

            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowAnyOrigin();
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Aurora", Version = "v1" });
            });

            var scrapers = OptionsScraperCollector.DiscoverScrapers(services);
            foreach (var scraper in scrapers)
            {
                services.AddTransient(scraper);
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseCors(CorsPolicyName);
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aurora v1"));

            app.UseAuthentication();

            app.UseHangfireDashboard(options: new DashboardOptions()
            {
                IgnoreAntiforgeryToken = true,
                Authorization = new[] { new HangfireAuthorization() }
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("hub/notifications");
            });
        }
    }
}
