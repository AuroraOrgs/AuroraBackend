namespace Aurora.Infrastructure.Config
{
    [ConfigSectionAttribute("Selenium")]
    public class SeleniumConfig
    {
        public string SeleniumLocation { get; set; }
    }
}
