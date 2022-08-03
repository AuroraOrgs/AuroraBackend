using Aurora.Application.Scrapers;
using Aurora.Infrastructure;
using Aurora.Infrastructure.Config;
using Aurora.Infrastructure.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Aurora.Presentation.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static IServiceProvider MigrateDatabase(this IServiceProvider provider)
        {
            var db = provider.GetRequiredService<SearchContext>();
            db.Database.Migrate();
            return provider;
        }

        /// <summary>
        /// This method would start recurring jobs for total scrapers and data refresh with regular scrapers
        /// </summary>
        public static IServiceProvider StartRecurringJobs(this IServiceProvider provider)
        {
            var totalConfig = provider.GetRequiredService<IOptions<TotalScraperConfig>>().Value;
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

            //TODO: Add refresh job

            return provider;
        }
    }
}
