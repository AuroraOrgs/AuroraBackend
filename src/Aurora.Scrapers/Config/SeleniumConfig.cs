using Aurora.Shared.Config;
using System.ComponentModel.DataAnnotations;

namespace Aurora.Scrapers.Config;

[ConfigSection("Selenium")]
public class SeleniumConfig
{
    [Required, Url]
    public string SeleniumLocation { get; set; } = null!;
}
