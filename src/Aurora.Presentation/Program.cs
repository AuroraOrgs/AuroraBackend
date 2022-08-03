using Aurora.Application.Scrapers;
using Aurora.Infrastructure;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;

namespace Aurora.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using IHost host = CreateHostBuilder(args).Build();
                using (var scope = host.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<SearchContext>();
                    db.Database.Migrate();

                    var totalConfig = scope.ServiceProvider.GetRequiredService<IOptions<TotalScraperConfig>>().Value;
                    var scrapers = OptionsScraperCollector.TotalScrapers;
                    foreach (var scraper in scrapers)
                    {
                        var jobId = scraper.Name;
                        string cron;
                        if (totalConfig.ScraperJobCrons.TryGetValue(jobId, out cron!) == false)
                        {
                            cron = totalConfig.BaseJobCron;
                        }
                        RecurringJob.RemoveIfExists(jobId);
                        RecurringJob.AddOrUpdate<ITotalScraperRunner>(runner => runner.RunTotalScraper(scraper), cron);
                    }
                }
                host.Run();
            }
            catch (Exception ex)
            {
                if (Log.Logger == null || Log.Logger.GetType().Name == "SilentLogger")
                {
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Console()
                        .CreateLogger();
                }

                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                   {
                       webBuilder
                        .UseStartup<Startup>();
                   })
                .UseSerilog((hostingContext, loggerConfiguration) =>
                   {
                       loggerConfiguration
                           .ReadFrom.Configuration(hostingContext.Configuration)
                           .Enrich.FromLogContext()
                           .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name!)
                           .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment);
                   });
    }
}