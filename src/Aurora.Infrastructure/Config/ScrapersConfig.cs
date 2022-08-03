namespace Aurora.Infrastructure.Config
{
    [ConfigSection(SectionName)]
    public class ScrapersConfig
    {
        public const string SectionName = "Scraper";

        public bool UseLimitations { get; set; } = true;
        public int MaxPagesCount { get; set; } = 5;
        public int MaxItemsCount { get; set; } = 200;
        public TotalScraperConfig Total { get; set; }
    }
}
