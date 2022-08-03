using Hangfire;
using System.Collections.Generic;

namespace Aurora.Infrastructure.Config
{
    [ConfigSection("Scraper", "Total")]
    public class TotalScraperConfig
    {
        /// <summary>
        /// Job Cron expression used, when no other is provided
        /// </summary>
        public string BaseJobCron { get; set; } = Cron.Daily();
        /// <summary>
        /// Job Cron expression override based on the scraper type name
        /// </summary>
        public Dictionary<string, string> ScraperJobCrons { get; set; } = new Dictionary<string, string>();
    }
}
