namespace Aurora.Infrastructure.Config
{
    [ConfigSection("Scraper")]
    public class ScrapersConfig
    {
        public bool UseLimitations { get; set; } = true;
        public int MaxPagesCount { get; set; } = 5;
        public int MaxItemsCount { get; set; } = 200;
    }
}
