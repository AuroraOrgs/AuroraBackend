using Aurora.Shared.Config;
using Hangfire;

namespace Aurora.Infrastructure.Config;

[ConfigSection("Refresh")]
public class RefreshConfig
{
    public int RefreshOptionsCount { get; set; } = 32;
    public bool UseRefresh { get; set; } = true;
    public string RefreshCron { get; set; } = Cron.Daily();
}
