using Aurora.Application.Scrapers;
using Aurora.Infrastructure;
using Aurora.Infrastructure.Config;
using Aurora.Scrapers.Config;
using Aurora.Scrapers.Discovery;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aurora.Presentation.Extensions;

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
        StartTotalScrapers(provider);
        StartRefreshJobs(provider);
        return provider;
    }

    private static void StartTotalScrapers(IServiceProvider provider)
    {
        var totalConfig = provider.GetRequiredService<IOptions<TotalScrapersConfig>>().Value;
        var ctx = provider.GetRequiredService<ScrapersContext>();
        var scrapers = ctx.TotalScrapers;
        foreach (var scraper in scrapers)
        {
            var jobId = scraper.Name;
            string cron;
            if (totalConfig.ScraperJobCrons.TryGetValue(jobId, out cron!) == false)
            {
                cron = totalConfig.BaseJobCron;
            }
            if (totalConfig.UseRecurringJob)
            {
                RecurringJob.AddOrUpdate<ITotalScraperRunner>(jobId, runner => runner.RunTotalScraper(scraper), cron);
            }
            else
            {
                RecurringJob.RemoveIfExists(jobId);
            }
        }
    }

    private static void StartRefreshJobs(IServiceProvider provider)
    {
        var config = provider.GetRequiredService<IOptions<RefreshConfig>>().Value;
        var refreshJobId = "RefreshJob";
        if (config.UseRefresh)
        {
            RecurringJob.AddOrUpdate<IRefreshRunner>(refreshJobId, runner => runner.RefreshAsync(CancellationToken.None), config.RefreshCron);
        }
        else
        {
            RecurringJob.RemoveIfExists(refreshJobId);
        }
    }
}
