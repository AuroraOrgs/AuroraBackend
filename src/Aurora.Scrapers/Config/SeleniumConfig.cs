using Aurora.Shared.Config;

namespace Aurora.Scrapers.Config
{
    [ConfigSection("Selenium")]
    public class SeleniumConfig
    {
        public string SeleniumLocation { get; set; } = "";
    }
}
