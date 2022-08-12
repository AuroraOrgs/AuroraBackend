using Aurora.Shared.Config;
using Hangfire;

namespace Aurora.Scrapers.Config
{
    [ConfigSection(ScrapersConfig.SectionName, nameof(ScrapersConfig.Total))]
    public class TotalScrapersConfig
    {
        public bool UseRecurringJob { get; set; } = false;
        /// <summary>
        /// Job Cron expression used, when no other is provided
        /// </summary>
        public string BaseJobCron { get; set; } = Cron.Daily();
        /// <summary>
        /// Job Cron expression override based on the scraper type name
        /// </summary>
        public Dictionary<string, string> ScraperJobCrons { get; set; } = new Dictionary<string, string>();

        public bool UseLimitations { get; set; } = true;
        public int MaxPagesCount { get; set; } = 100;
    }
}
