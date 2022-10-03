using Aurora.Shared.Config;

namespace Aurora.Infrastructure.Config;

[ConfigSection("Refresh")]
public class RefreshConfig
{
    public int RefreshCount { get; set; } = 32;
}
