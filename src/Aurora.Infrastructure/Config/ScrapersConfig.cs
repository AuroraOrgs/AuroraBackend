namespace Aurora.Infrastructure.Config
{
    [ConfigSection("Scraper")]
    public class ScrapersConfig
    {
        public int MaxPagesCount { get; set; } = 5;
        public int MaxItemsCount { get; set; } = 200;
    }
}
