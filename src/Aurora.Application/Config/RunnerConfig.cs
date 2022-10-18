using Aurora.Shared.Config;
using System.ComponentModel.DataAnnotations;

namespace Aurora.Application.Config;

[ConfigSection("Scrapers", "Runner")]
public class RunnerConfig
{
    [Range(1, Int32.MaxValue)]
    public int MaxConcurrentScrapers { get; set; } = 5;
}
